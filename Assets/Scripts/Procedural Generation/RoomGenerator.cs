using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SimpleJSON;

public class RoomMetadata {
    public int widthInCells;
    public int heightInCells;
    public int startHeight;
    public int endHeight;
    public GameObject room;

    public RoomMetadata(int widthInCells, int heightInCells, int startHeight, int endHeight, GameObject room) {
        this.widthInCells = widthInCells;
        this.heightInCells = heightInCells;
        this.startHeight = startHeight;
        this.endHeight = endHeight;
        this.room = room;
    }
}

public class RoomGeneratorConfiguration {
    public float floorHeightChangeProbability = 0f;
    public float probabilityOfEnemySpawn;

    public static RoomGeneratorConfiguration LoadFromJSONNode(JSONNode node) {
        float floorHeightChangeProbability = node ["floorHeightChangeProbability"].AsFloat;
        float enemySpawnProbability = node ["enemySpawnProbability"].AsFloat;

        // @TODO Load enemy prefabs dynamically

        return new RoomGeneratorConfiguration (floorHeightChangeProbability, enemySpawnProbability);
    }

    public RoomGeneratorConfiguration(float floorHeightChangeProbability, float probabilityOfEnemySpawn) {
        this.floorHeightChangeProbability = floorHeightChangeProbability;
        this.probabilityOfEnemySpawn = probabilityOfEnemySpawn;
    }
}

public static class RoomGenerator {
  
    public static RoomMetadata GenerateRoom(string roomName, Vector2 bottomLeftCorner, LevelConfiguration levelConfig, LevelZone zone, GameObject[] platformPrefabs, RoomSpawnTrigger roomSpawnTrigger, NewZoneTrigger newZoneTrigger, EnemyHordeMember[] enemyPrefabs) {
        GameObject root = new GameObject ();
        root.name = roomName;
        root.transform.position = bottomLeftCorner;
    
        // Choose platform size
        float roomWidthInUnits = levelConfig.roomCellsWide * levelConfig.cellWidth;
        float roomHeightInUnits = levelConfig.roomCellsTall * levelConfig.cellHeight;

        // Choose random assortment of mini-platforms to fill platform size
        List<int> platformWidths = new List<int>();
        Dictionary<int, GameObject> widthToPlatformMap = new Dictionary<int, GameObject> ();
        string platformPattern = ".*-(\\d+)x(\\d+)$";
        Regex platformMatcher = new Regex(platformPattern);
        for (int i = 0; i < platformPrefabs.Length; ++i) {
            if (platformPrefabs[i] == null) {
                Debug.LogError ("Room Generation error: Platform prefab at position " + i + " is null.");
                continue;
            }
            Match result = platformMatcher.Match (platformPrefabs[i].name);
            if (result.Success) {
                int platformWidth = int.Parse (result.Groups [1].Value);
                platformWidths.Add (platformWidth);
                widthToPlatformMap.Add (platformWidth, platformPrefabs[i]);
            }
        }
        platformWidths.Sort ();

        // Dump the widths to the console.
        // Debug.Log ("Available widths: " + RoomGenerator.StringifyArray<int>(platformWidths.ToArray ()));

        int cellsRemaining = levelConfig.roomCellsWide;
        List<int> platformsToUse = new List<int> ();
        while (cellsRemaining > 0) {
            int maxWidthIndex = 0;
            for (int i = 0; i < platformWidths.Count; ++i) {
                if (platformWidths[i] <= cellsRemaining) {
                    maxWidthIndex = i;
                }
            }
            int index = Random.Range (0, maxWidthIndex + 1);
            platformsToUse.Add (platformWidths [index]);
            cellsRemaining -= platformWidths [index];
        }

        // Shuffle the platform arrangement
        for (int i = 0; i < platformsToUse.Count; i++) {
            int temp = platformsToUse[i];
            int randomIndex = Random.Range(i, platformsToUse.Count);
            platformsToUse[i] = platformsToUse[randomIndex];
            platformsToUse[randomIndex] = temp;
        }
        // Debug.Log ("Chosen widths: " + RoomGenerator.StringifyArray<int>(platformsToUse.ToArray ()));

        // Randomly generate mini-platform heights along grid (min / max deviation parameters, and hard level-min / level max.
        int[] floorHeights = new int[platformsToUse.Count];
        int lastCellHeight = 0;
        for (int i = 0; i < platformsToUse.Count; ++i) {
            float prob = Random.Range (0f, 1f);
            if (prob <= zone.roomConfig.floorHeightChangeProbability) {
                if (prob < zone.roomConfig.floorHeightChangeProbability / 2f) {
                    floorHeights [i] = lastCellHeight + 1;    
                }
                else {
                    floorHeights [i] = lastCellHeight - 1;
                }
            }
            else {
                floorHeights [i] = lastCellHeight;
            }

            lastCellHeight = floorHeights [i];
        }
        // Debug.Log ("Heights: " + RoomGenerator.StringifyArray<int>(floorHeights));

        // Create and position platforms
        float startX = 0;
        for (int i = 0; i < platformsToUse.Count; ++i) {
            int width = platformsToUse [i];
            GameObject platform = GameObject.Instantiate<GameObject> (widthToPlatformMap [width]);
            platform.transform.SetParent (root.transform, false);
            platform.transform.localPosition = new Vector2 (startX, floorHeights[i] * levelConfig.cellHeight);
            platform.transform.localScale = new Vector2(levelConfig.cellWidth, levelConfig.cellHeight);
            platform.name = "Floor-" + i;

            Sprite floorSprite = Resources.Load<Sprite> (zone.spriteSetPrefix + "/Ground-" + width + "x1");
            if (floorSprite != null) {
                platform.GetComponent<SpriteRenderer> ().sprite = floorSprite;
            }

            if (zone.needsCeiling) {
                GameObject ceilingPlatform = GameObject.Instantiate<GameObject>(widthToPlatformMap[width]);
                ceilingPlatform.transform.SetParent(root.transform, false);
                ceilingPlatform.transform.localPosition = new Vector2(startX, floorHeights[i] * levelConfig.cellHeight + roomHeightInUnits);
                ceilingPlatform.transform.localScale = new Vector2(levelConfig.cellWidth, levelConfig.cellHeight);
                ceilingPlatform.name = "ceiling-" + i;
                Sprite ceilingSprite = Resources.Load<Sprite>(zone.spriteSetPrefix + "/Ground-" + width + "x1");
                if (ceilingSprite != null) {
                    ceilingPlatform.GetComponent<SpriteRenderer>().sprite = ceilingSprite;
                }
            }

            startX += width * levelConfig.cellWidth;
        }

        // Generate spawn trigger.
        if (roomSpawnTrigger != null) {
            int triggerOriginCell = Mathf.FloorToInt(levelConfig.roomCellsWide * levelConfig.spawnTriggerRoomLocation);
            if (triggerOriginCell == levelConfig.roomCellsWide) { // Account for situation where spawnTriggerWidthPercentage is precisely 1.0 and the array index is out of range.
                triggerOriginCell--;
            }
            int triggerOriginPlatform = 0;
            int cellsCounted = 0;
            for (int i = 0; i < platformsToUse.Count; ++i) {
                cellsCounted += platformsToUse [i];
                if (triggerOriginCell < cellsCounted) {
                    break;
                }
                else {
                    triggerOriginPlatform++;
                }
            }

            float triggerHeight = floorHeights [triggerOriginPlatform] * levelConfig.cellHeight + roomHeightInUnits / 2f;
            Vector2 triggerOrigin = new Vector2(roomWidthInUnits * levelConfig.spawnTriggerRoomLocation, triggerHeight);
            RoomSpawnTrigger trigger = GameObject.Instantiate<RoomSpawnTrigger> (roomSpawnTrigger);
            trigger.transform.SetParent (root.transform, false);
            trigger.transform.localPosition = triggerOrigin;
            trigger.transform.localScale = new Vector2 (1f, roomHeightInUnits);    
        }

        // If this isn't null, we're entering a new zone.
        if (newZoneTrigger != null) {
            float triggerHeight = floorHeights[0] * levelConfig.cellHeight + roomHeightInUnits / 2f;
            Vector2 triggerOrigin = new Vector2(0, triggerHeight);
            NewZoneTrigger trigger = GameObject.Instantiate<NewZoneTrigger>(newZoneTrigger);
            trigger.transform.SetParent(root.transform, false);
            trigger.transform.localPosition = triggerOrigin;
            trigger.transform.localScale = new Vector2(1f, roomHeightInUnits);
            trigger.zone = zone;
        }

        // Generate enemies
        float enemyOdds = Random.Range (0f, 1f);
        if (enemyOdds <= zone.roomConfig.probabilityOfEnemySpawn) {
            int enemyIndex = Random.Range (0, enemyPrefabs.Length);
            EnemyHordeMember enemy = GameObject.Instantiate<EnemyHordeMember> (enemyPrefabs[enemyIndex]);
            enemy.transform.SetParent (root.transform, false);
            enemy.transform.localPosition = new Vector2 (roomWidthInUnits / 2f, levelConfig.roomCellsTall * levelConfig.cellHeight / 2f);

            // @TODO Face left.
        }

        RoomMetadata room = new RoomMetadata (levelConfig.roomCellsWide, levelConfig.roomCellsTall, floorHeights [0], floorHeights [floorHeights.Length - 1], root);
        return room;
    }

    static string StringifyArray<T>(T[] data) {
        string output = "";
        for (int i = 0; i < data.Length; ++i) {
            output += data [i].ToString () + ", ";
        }
        return output;
    }
}
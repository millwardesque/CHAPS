using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public struct RoomMetadata {
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
    public string name;
    public Vector2 bottomLeftCorner;

    public float cellWidth = 1f;
    public float cellHeight = 0.5f;
    public int minCellsWide = 8;
    public int maxCellsWide = 16;
    public int roomHeightInCells = 12;

    public float floorHeightChangeProbability = 0f;
    public float spawnTriggerWidthPercentage = 0.9f;

    public GameObject[] platformPrefabs;

    public RoomGeneratorConfiguration(string name, Vector2 bottomLeftCorner, int minCellsWide, int maxCellsWide, int roomHeightInCells, float cellWidth, float cellHeight, GameObject[] platformPrefabs, float floorHeightChangeProbability, float spawnTriggerWidthPercentage) {
        this.name = name;
        this.bottomLeftCorner = bottomLeftCorner;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
        this.minCellsWide = minCellsWide;
        this.maxCellsWide = maxCellsWide;
        this.roomHeightInCells = roomHeightInCells;
        this.floorHeightChangeProbability = floorHeightChangeProbability;
        this.spawnTriggerWidthPercentage = spawnTriggerWidthPercentage;
        this.platformPrefabs = platformPrefabs;
    }
}

public static class RoomGenerator {
  
    public static RoomMetadata GenerateRoom(RoomGeneratorConfiguration config) {
        GameObject root = new GameObject ();
        root.name = config.name;
        root.transform.position = config.bottomLeftCorner;
    
        // Choose platform size
        int roomWidthInCells = Random.Range (config.minCellsWide, config.maxCellsWide);
        float roomWidthInUnits = roomWidthInCells * config.cellWidth;
        float roomHeightInUnits = config.roomHeightInCells * config.cellHeight;

        // Debug.Log (string.Format ("Room dimensions: {0} x {1} cells ({2} x {3} units)", roomWidthInCells, roomHeightInCells, roomWidthInUnits, roomHeightInUnits));

        // Choose random assortment of mini-platforms to fill platform size
        List<int> platformWidths = new List<int>();
        Dictionary<int, GameObject> widthToPlatformMap = new Dictionary<int, GameObject> ();
        string platformPattern = ".*-(\\d+)x(\\d+)$";
        Regex platformMatcher = new Regex(platformPattern);
        for (int i = 0; i < config.platformPrefabs.Length; ++i) {
            if (config.platformPrefabs[i] == null) {
                Debug.LogError ("Room Generation error: Platform prefab at position " + i + " is null.");
                continue;
            }
            Match result = platformMatcher.Match (config.platformPrefabs[i].name);
            if (result.Success) {
                int platformWidth = int.Parse (result.Groups [1].Value);
                platformWidths.Add (platformWidth);
                widthToPlatformMap.Add (platformWidth, config.platformPrefabs[i]);
            }
        }
        platformWidths.Sort ();

        // Dump the widths to the console.
        // Debug.Log ("Available widths: " + RoomGenerator.StringifyArray<int>(platformWidths.ToArray ()));

        int cellsRemaining = roomWidthInCells;
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
            if (prob <= config.floorHeightChangeProbability) {
                if (prob < config.floorHeightChangeProbability / 2f) {
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
            platform.transform.localPosition = new Vector2 (startX, floorHeights[i] * config.cellHeight);
            platform.name = "Floor-" + i;

            GameObject ceilingPlatform = GameObject.Instantiate<GameObject> (widthToPlatformMap [width]);
            ceilingPlatform.transform.SetParent (root.transform, false);
            ceilingPlatform.transform.localPosition = new Vector2 (startX, floorHeights[i] * config.cellHeight + roomHeightInUnits);
            ceilingPlatform.name = "ceiling-" + i;

            startX += width * config.cellWidth;
        }

        // @TODO Generate spawn trigger.

        RoomMetadata room = new RoomMetadata (roomWidthInCells, config.roomHeightInCells, floorHeights [0], floorHeights [floorHeights.Length - 1], root);
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
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

public class RoomGenerator : MonoBehaviour {
    [Header("General")]
    [Range(0.25f, 5f)]
    public float cellWidth = 1f;

    [Range(0.25f, 5f)]
    public float cellHeight = 0.5f;

    [Header("Room Size")]
    [Range(1, 32)]
    public int minCellsWide = 8;

    [Range(1, 32)]
    public int maxCellsWide = 16;

    [Range(1, 32)]
    public int minCellsTall = 8;

    [Range(1, 32)]
    public int maxCellsTall = 12;

    [Header("Room Geometry")]
    public GameObject[] platformPrefabs;

    [Range(0f, 1f)]
    public float floorHeightChangeProbability = 0f;

    public RoomMetadata GenerateRoom(string name, Vector2 bottomLeftCorner) {
        GameObject root = new GameObject ();
        root.name = name;
        root.transform.position = bottomLeftCorner;
    
        // Choose platform size
        int roomWidthInCells = Random.Range (minCellsWide, maxCellsWide);
        float roomWidthInUnits = roomWidthInCells * cellWidth;

        int roomHeightInCells = Random.Range (minCellsTall, maxCellsTall);
        float roomHeightInUnits = roomHeightInCells * cellHeight;

        Debug.Log (string.Format ("Room dimensions: {0} x {1} cells ({2} x {3} units)", roomWidthInCells, roomHeightInCells, roomWidthInUnits, roomHeightInUnits));

        // Choose random assortment of mini-platforms to fill platform size
        List<int> platformWidths = new List<int>();
        Dictionary<int, GameObject> widthToPlatformMap = new Dictionary<int, GameObject> ();
        string platformPattern = ".*-(\\d+)x(\\d+)$";
        Regex platformMatcher = new Regex(platformPattern);
        for (int i = 0; i < platformPrefabs.Length; ++i) {
            if (platformPrefabs[i] == null) {
                Debug.Log ("Room Generation error: Platform prefab at position " + i + " is null.");
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
        Debug.Log ("Available widths: " + StringifyArray<int>(platformWidths.ToArray ()));

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
        Debug.Log ("Chosen widths: " + StringifyArray<int>(platformsToUse.ToArray ()));

        // Randomly generate mini-platform heights along grid (min / max deviation parameters, and hard level-min / level max.
        int[] floorHeights = new int[platformsToUse.Count];
        int lastCellHeight = 0;
        for (int i = 0; i < platformsToUse.Count; ++i) {
            float prob = Random.Range (0f, 1f);
            if (prob <= floorHeightChangeProbability) {
                if (prob < floorHeightChangeProbability / 2f) {
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
        Debug.Log ("Heights: " + StringifyArray<int>(floorHeights));

        // Create and position platforms
        float startX = 0;
        for (int i = 0; i < platformsToUse.Count; ++i) {
            int width = platformsToUse [i];
            GameObject platform = Instantiate<GameObject> (widthToPlatformMap [width]);
            platform.transform.SetParent (root.transform, false);
            platform.transform.localPosition = new Vector2 (startX, floorHeights[i] * cellHeight);

            GameObject ceilingPlatform = Instantiate<GameObject> (widthToPlatformMap [width]);
            ceilingPlatform.transform.SetParent (root.transform, false);
            ceilingPlatform.transform.localPosition = new Vector2 (startX, floorHeights[i] * cellHeight + roomHeightInUnits);

            startX += width * cellWidth;

        }

        RoomMetadata room = new RoomMetadata (roomWidthInCells, roomHeightInCells, floorHeights [0], floorHeights [floorHeights.Length - 1], root);
        return room;
    }

    public GameObject GenerateRooms(int roomCount, string name, Vector2 bottomLeftCorner) {
        GameObject root = new GameObject ();
        root.name = name;
        root.transform.position = bottomLeftCorner;

        Vector2 start = Vector2.zero;
        for (int i = 0; i < roomCount; ++i) {
            RoomMetadata room = GenerateRoom (name + "-" + i, start);
            room.room.transform.SetParent (root.transform, false);
            start += new Vector2 (room.widthInCells * cellWidth, room.endHeight * cellHeight);
        }

        return root;
    }

    string StringifyArray<T>(T[] data) {
        string output = "";
        for (int i = 0; i < data.Length; ++i) {
            output += data [i].ToString () + ", ";
        }
        return output;
    }
}
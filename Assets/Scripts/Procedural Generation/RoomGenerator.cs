using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

    public GameObject GenerateRoom(string name, Vector2 bottomLeftCorner) {
        GameObject root = new GameObject ();
        root.name = name;
        root.transform.position = bottomLeftCorner;
    
        // Choose platform size
        int roomWidthInCells = Random.Range (minCellsWide, maxCellsWide);
        float roomWidthInUnits = roomWidthInCells * cellWidth;

        int roomHeightInCells = Random.Range (minCellsTall, maxCellsTall);
        float roomHeightInUnits = roomHeightInCells * cellHeight;

        Debug.Log (string.Format ("Room dimensions: {0} x {1} cells ({2} x {3} units)", roomWidthInCells, roomHeightInCells, roomWidthInUnits, roomHeightInUnits));

        // @TODO - Choose random assortment of mini-platforms to fill platform size
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
        string widthString = "";
        for (int i = 0; i < platformWidths.Count; ++i) {
            widthString += platformWidths [i] + ", ";
        }
        Debug.Log ("Available widths: " + widthString);

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

        // Dump the chosen platform widths to the console.
        string platformWidthString = "";
        for (int i = 0; i < platformsToUse.Count; ++i) {
            platformWidthString += platformsToUse [i] + ", ";
        }
        Debug.Log ("Chosen widths: " + platformWidthString);

        // @TODO - Randomly generate mini-platform heights along grid (min / max deviation parameters, and hard level-min / level max.
        // @TODO - Position side-by-side
        // @TODO - Create ceiling tiles that mimic floor positions but are a fixed height away

        return root;
    }
}
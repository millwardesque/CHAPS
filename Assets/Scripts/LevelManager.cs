using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    [Header("General")]
    [Range(0.25f, 5f)]
    public float cellWidth = 1f;

    [Range(0.25f, 5f)]
    public float cellHeight = 0.5f;

    [Range(0f, 1f)]
    public float spawnTriggerWidthPercentage = 0.9f;

    [Header("Room Size")]
    [Range(1, 32)]
    public int minCellsWide = 8;

    [Range(1, 32)]
    public int maxCellsWide = 16;

    [Range(1, 32)]
    public int roomHeightInCells = 12;

    [Header("Room Geometry")]
    public GameObject[] platformPrefabs;

    [Range(0f, 1f)]
    public float floorHeightChangeProbability = 0f;

    public GameObject GenerateRooms(int roomCount, string name, Vector2 bottomLeftCorner) {
        GameObject root = new GameObject ();
        root.name = name;
        root.transform.position = bottomLeftCorner;

        Vector2 start = Vector2.zero;
        for (int i = 0; i < roomCount; ++i) {
            RoomMetadata room = RoomGenerator.GenerateRoom (GetConfigurationOptions(name + "-" + i, start));
            room.room.transform.SetParent (root.transform, false);
            start += new Vector2 (room.widthInCells * cellWidth, room.endHeight * cellHeight);
        }

        return root;
    }

    RoomGeneratorConfiguration GetConfigurationOptions(string roomName, Vector2 bottomLeftCorner) {
        return new RoomGeneratorConfiguration(name, bottomLeftCorner, minCellsWide, maxCellsWide, roomHeightInCells, cellWidth, cellHeight, platformPrefabs, floorHeightChangeProbability, spawnTriggerWidthPercentage);
    }
}

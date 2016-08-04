using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    [Header("General")]
    [Range(0.25f, 5f)]
    public float cellWidth = 1f;

    [Range(0.25f, 5f)]
    public float cellHeight = 0.5f;
        
    [Header("Room Spawning")]
    [Range(0f, 1f)]
    public float spawnTriggerWidthPercentage = 0.9f;

    [Range(1, 12)]
    public int roomsToSpawn = 1;

    public RoomSpawnTrigger roomSpawnTrigger;

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

    RoomMetadata m_lastRoom;
    Vector2 m_nextRoomStart;

    void Awake() {
        GameManager.Instance.Messenger.AddListener ("RoomSpawnTrigger", OnRoomSpawnTrigger);
    }

    public GameObject GenerateRooms(string name, int roomCount = -1) {
        if (roomCount == -1) {
            roomCount = roomsToSpawn;
        }

        GameObject root = new GameObject ();
        root.name = name;

        for (int i = 0; i < roomCount; ++i) {
            RoomMetadata room = RoomGenerator.GenerateRoom (GetConfigurationOptions(name + "-" + i, m_nextRoomStart));
            room.room.transform.SetParent (root.transform, false);
            m_nextRoomStart += new Vector2 (room.widthInCells * cellWidth, room.endHeight * cellHeight);
            m_lastRoom = room;
        }

        return root;
    }

    RoomGeneratorConfiguration GetConfigurationOptions(string roomName, Vector2 bottomLeftCorner) {
        return new RoomGeneratorConfiguration(roomName, bottomLeftCorner, minCellsWide, maxCellsWide, roomHeightInCells, cellWidth, cellHeight, platformPrefabs, floorHeightChangeProbability, spawnTriggerWidthPercentage, roomSpawnTrigger);
    }

    void OnRoomSpawnTrigger(Message message) {
        GenerateRooms ("Spawned room");
    }
}

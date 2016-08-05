using UnityEngine;
using System.Collections;

public class LevelManager : MonoBehaviour {
    [Header("General")]
    [Range(0.25f, 5f)]
    public float cellWidth = 1f;

    [Range(0.25f, 5f)]
    public float cellHeight = 0.5f;

    public string roomType = "Prototype";
        
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

    [Header("Safe room")]
    [Range(0, 128)]
    public int roomsBetweenSafeRooms = 12;
    public GameObject safeRoomPrefab;

    [Header("Enemies")]
    [Range(0f, 1f)]
    public float probabilityOfEnemySpawn = 0.2f;
    public EnemyHordeMember[] enemyPrefabs;


    RoomMetadata m_lastRoom;
    Vector2 m_nextRoomStart;
    int m_roomsSinceSafeRoom;

    void Awake() {
        GameManager.Instance.Messenger.AddListener ("RoomSpawnTrigger", OnRoomSpawnTrigger);
        GameManager.Instance.Messenger.AddListener ("SafeRoomExit", OnSafeRoomExit);
        m_roomsSinceSafeRoom = 0;
    }

    public GameObject GenerateRooms(string name, int roomCount = -1) {
        if (roomCount == -1) {
            roomCount = roomsToSpawn;
        }

        GameObject root = new GameObject ();
        root.name = name;

        for (int i = 0; i < roomCount; ++i) {
            if (m_roomsSinceSafeRoom == roomsBetweenSafeRooms) {
                GameObject safeRoom = Instantiate <GameObject> (safeRoomPrefab);
                safeRoom.transform.SetParent (root.transform, false);
                safeRoom.transform.localPosition = m_nextRoomStart;

                // Figure out the width of the safe room
                Collider2D[] colliders = safeRoom.GetComponentsInChildren<Collider2D> ();
                float safeRoomWidth = 0f;
                for (int j = 0; j < colliders.Length; ++j) {
                    float colliderX = colliders [j].bounds.max.x;
                    if (colliderX > safeRoomWidth) {
                        safeRoomWidth = colliderX;
                    }
                }
                m_nextRoomStart = new Vector2 (safeRoomWidth, m_nextRoomStart.y);
                m_roomsSinceSafeRoom = 0;
            }
            else {
                RoomMetadata room = RoomGenerator.GenerateRoom (GetConfigurationOptions(name + "-" + i, m_nextRoomStart));
                room.room.transform.SetParent (root.transform, false);
                m_nextRoomStart += new Vector2 (room.widthInCells * cellWidth, room.endHeight * cellHeight);
                m_lastRoom = room;
                m_roomsSinceSafeRoom++;
            }
        }

        return root;
    }

    RoomGeneratorConfiguration GetConfigurationOptions(string roomName, Vector2 bottomLeftCorner) {
        return new RoomGeneratorConfiguration(roomName, bottomLeftCorner, minCellsWide, maxCellsWide, roomHeightInCells, cellWidth, cellHeight, platformPrefabs, floorHeightChangeProbability, spawnTriggerWidthPercentage, roomSpawnTrigger, probabilityOfEnemySpawn, enemyPrefabs, roomType);
    }

    void OnRoomSpawnTrigger(Message message) {
        GenerateRooms ("Spawned room", 1);
    }

    void OnSafeRoomExit(Message message) {
        Debug.Log ("@TODO Cleanup old rooms here");
    }
}

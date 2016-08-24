using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class LevelConfiguration {
    public string skin;
    public float cellWidth;
    public float cellHeight;
    public float spawnTriggerRoomLocation;
    public int roomsToSpawnAtOnce;
    public int roomCellsWide;
    public int roomCellsTall;

    public static LevelConfiguration LoadFromJSONNode(JSONNode node) {
        string skin = node["skin"];
        float cellWidth = node ["cellWidth"].AsFloat;
        float cellHeight = node ["cellHeight"].AsFloat;
        float spawnTriggerRoomLocation = node ["spawnTriggerRoomLocation"].AsFloat;
        int roomsToSpawnAtOnce = node ["roomsToSpawnAtOnce"].AsInt;
        int roomCellsWide = node ["roomCellsWide"].AsInt;
        int roomCellsTall = node ["roomCellsTall"].AsInt;

        return new LevelConfiguration (skin, cellWidth, cellHeight, spawnTriggerRoomLocation, roomsToSpawnAtOnce, roomCellsWide, roomCellsTall);
    }

    public LevelConfiguration(string skin, float cellWidth, float cellHeight, float spawnTriggerRoomLocation, int roomsToSpawnAtOnce, int roomCellsWide, int roomCellsTall) {
        this.skin = skin;
        this.cellWidth = cellWidth;
        this.cellHeight = cellHeight;
        this.spawnTriggerRoomLocation = spawnTriggerRoomLocation;
        this.roomsToSpawnAtOnce = roomsToSpawnAtOnce;
        this.roomCellsWide = roomCellsWide;
        this.roomCellsTall = roomCellsTall;
    }

    public override string ToString() {
        return string.Format ("Skin: {0}, Cell Dimensions {1} x {2}, Room dimensions {3} x {4}, Spawn Trigger location: {5}, Rooms to spawn at once: {6}", skin, cellWidth, cellHeight, roomCellsWide, roomCellsTall, spawnTriggerRoomLocation, roomsToSpawnAtOnce);
    }
}

public class LevelManager : MonoBehaviour {
    public string levelResourceFile;
    public int roomCleanupDistance = 10;   // Number of rooms behind the player a room has to be before it gets cleaned up.

    [Header("Prefabs")]
    public RoomSpawnTrigger roomSpawnTrigger;
    public GameObject[] platformPrefabs;
    public EnemyHordeMember[] enemyPrefabs;
    public GameObject safeRoomPrefab;

    LevelConfiguration m_levelConfiguration;
    LevelZone[] m_zones;
    int m_activeZone = 0;

    List<RoomMetadata> m_generatedRooms;
    Vector2 m_nextGeneratedRoomStart;
    int m_roomsSinceSafeRoom;


    void Awake() {
        GameManager.Instance.Messenger.AddListener ("RoomSpawnTrigger", OnRoomSpawnTrigger);
        m_roomsSinceSafeRoom = 0;

        m_generatedRooms = new List<RoomMetadata> ();

        LoadFromFile(levelResourceFile);
        m_activeZone = 0;
    }

    void Start() {
        ActivateZone(0);
    }

    public void LoadFromFile(string resourceName) {
        TextAsset jsonAsset = Resources.Load<TextAsset>(resourceName);
        if (jsonAsset != null) {
            string fileContents = jsonAsset.text;
            JSONNode node = JSON.Parse (fileContents);

            JSONNode levelConfiguration = node ["levelConfiguration"];
            m_levelConfiguration = LevelConfiguration.LoadFromJSONNode (levelConfiguration);
            Debug.Log ("Loaded level config: " + m_levelConfiguration.ToString ());

            JSONArray zoneArray = node ["zones"].AsArray;
            m_zones = new LevelZone[zoneArray.Count];
            for (int i = 0; i < zoneArray.Count; ++i) {
                m_zones[i] = LevelZone.LoadFromJSONNode (zoneArray[i]);
                Debug.Log ("Loaded zone: " + m_zones [i].ToString());
            }
        }
        else {
            Debug.LogError ("Unable to load level from file '" + resourceName + "'");
        }
    }

    public void GenerateRooms(string name, int roomCount = -1) {
        if (roomCount == -1) {
            roomCount = m_levelConfiguration.roomsToSpawnAtOnce;
        }

        for (int i = 0; i < roomCount; ++i) {
            if (m_roomsSinceSafeRoom == m_zones[m_activeZone].lengthInCells) {
                GameObject safeRoom = Instantiate <GameObject> (safeRoomPrefab);
                safeRoom.transform.SetParent (transform, false);
                safeRoom.transform.localPosition = m_nextGeneratedRoomStart;

                // Figure out the width of the safe room
                Collider2D[] colliders = safeRoom.GetComponentsInChildren<Collider2D> ();
                float safeRoomWidth = 0f;
                for (int j = 0; j < colliders.Length; ++j) {
                    float colliderX = colliders [j].bounds.max.x;
                    if (colliderX > safeRoomWidth) {
                        safeRoomWidth = colliderX;
                    }
                }
                m_nextGeneratedRoomStart = new Vector2 (safeRoomWidth, m_nextGeneratedRoomStart.y);
                m_roomsSinceSafeRoom = 0;

                // Change zones
                if (m_activeZone + 1 < m_zones.Length) {
                    ActivateZone(m_activeZone + 1);
                }
                else {
                    // @TODO Decide what to do now that we've hit the end of the level.
                }


                // @TODO Add safe-room to generated list so that it can get cleaned up.
            }
            else {
                // Disable enemy spawns if this is the first room in a zone.
                float tempEnemySpawnRate = m_zones [m_activeZone].roomConfig.probabilityOfEnemySpawn;
                if (m_roomsSinceSafeRoom == 0) {
                    m_zones [m_activeZone].roomConfig.probabilityOfEnemySpawn = 0f;
                }

                // Generate the room.
                RoomMetadata room = RoomGenerator.GenerateRoom (name + "-" + i, m_nextGeneratedRoomStart, m_levelConfiguration, m_zones[m_activeZone], platformPrefabs, roomSpawnTrigger, enemyPrefabs);

                // Restore enemy spawns if this was the first room in a zone.
                if (m_roomsSinceSafeRoom == 0) {
                    m_zones [m_activeZone].roomConfig.probabilityOfEnemySpawn = tempEnemySpawnRate;
                }

                room.room.transform.SetParent (transform, false);
                m_nextGeneratedRoomStart += new Vector2 (room.widthInCells * m_levelConfiguration.cellWidth, room.endHeight * m_levelConfiguration.cellHeight);
                m_roomsSinceSafeRoom++;
                m_generatedRooms.Add (room);
            }
        }
    }

    void OnRoomSpawnTrigger(Message message) {
        GenerateRooms ("Spawned room", 1);

        // Clean up old rooms
        RoomSpawnTrigger trigger = message.data as RoomSpawnTrigger;
        GameObject triggeredRoom = trigger.transform.parent.gameObject;
        for (int i = 0; i < m_generatedRooms.Count; ++i) {
            RoomMetadata room = m_generatedRooms [i];
            if (room.room == triggeredRoom) {
                if (i - roomCleanupDistance >= 0) {
                    CleanupRoomsBefore (i - roomCleanupDistance);
                }
                break;
            }
        }
    }

    void CleanupRoomsBefore(int roomIndex) {
        if (roomIndex >= 0) {
            for (int i = 0; i < roomIndex; ++i) {
                Destroy (m_generatedRooms [i].room);
                m_generatedRooms.RemoveAt(i);
            }
        }
    }

    void ActivateZone(int zoneIndex) {
        m_activeZone = zoneIndex;

        GameManager.Instance.backgroundMusic.clip = m_zones[m_activeZone].backgroundMusic;
        if (m_zones[m_activeZone].backgroundMusic != null) {
            GameManager.Instance.backgroundMusic.Play();
        }
    }
}

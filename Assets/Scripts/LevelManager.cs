using UnityEngine;
using System.Collections;
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

    [Header("Prefabs")]
    public RoomSpawnTrigger roomSpawnTrigger;
    public GameObject[] platformPrefabs;
    public EnemyHordeMember[] enemyPrefabs;

    [Header("Safe room")]
    [Range(0, 128)]
    public int roomsBetweenSafeRooms = 12;
    public GameObject safeRoomPrefab;

    LevelConfiguration m_levelConfiguration;
    LevelZone[] m_zones;
    int m_activeZone = 0;

    RoomMetadata m_lastRoom;
    Vector2 m_nextRoomStart;
    int m_roomsSinceSafeRoom;

    void Awake() {
        GameManager.Instance.Messenger.AddListener ("RoomSpawnTrigger", OnRoomSpawnTrigger);
        GameManager.Instance.Messenger.AddListener ("SafeRoomExit", OnSafeRoomExit);
        m_roomsSinceSafeRoom = 0;

        LoadFromFile(levelResourceFile);
        m_activeZone = 0;
    }

    void Start() {
        m_activeZone = 0;
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

    public GameObject GenerateRooms(string name, int roomCount = -1) {
        if (roomCount == -1) {
            roomCount = m_levelConfiguration.roomsToSpawnAtOnce;
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
                RoomMetadata room = RoomGenerator.GenerateRoom (name + "-" + i, m_nextRoomStart, m_levelConfiguration, m_zones[m_activeZone], platformPrefabs, roomSpawnTrigger, enemyPrefabs);
                room.room.transform.SetParent (root.transform, false);
                m_nextRoomStart += new Vector2 (room.widthInCells * m_levelConfiguration.cellWidth, room.endHeight * m_levelConfiguration.cellHeight);
                m_lastRoom = room;
                m_roomsSinceSafeRoom++;
            }
        }

        return root;
    }

    void OnRoomSpawnTrigger(Message message) {
        GenerateRooms ("Spawned room", 1);
    }

    void OnSafeRoomExit(Message message) {
        Debug.Log ("@TODO Cleanup old rooms here");
    }
}

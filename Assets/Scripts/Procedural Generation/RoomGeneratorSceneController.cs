using UnityEngine;
using System.Collections;

public class RoomGeneratorSceneController : MonoBehaviour {
    GameObject m_room;
    LevelManager m_generator;
    int m_generatedRooms = 0;
    int m_generatedLayouts = 0;

    [Range(1, 10)]
    public int roomsToGenerate = 4;
	
    // Use this for initialization
	void Start () {
        m_generator = GetComponent<LevelManager> ();
        m_generatedRooms = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.G)) {
            if (m_room != null) {
                Destroy (m_room);
            }
            m_room = m_generator.GenerateRooms (1, "Room " + m_generatedRooms, new Vector2(-8f, -4f));
            m_generatedRooms++;
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            if (m_room != null) {
                Destroy (m_room);
            }
            m_room = m_generator.GenerateRooms (roomsToGenerate, "Layout " + m_generatedLayouts, new Vector2 (-8f, -4f));
            m_generatedLayouts++;
        }
	}
}

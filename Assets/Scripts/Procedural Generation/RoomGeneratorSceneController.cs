using UnityEngine;
using System.Collections;

public class RoomGeneratorSceneController : MonoBehaviour {
    GameObject m_room;
    RoomGenerator m_generator;
    int m_generatedRooms = 0;
	
    // Use this for initialization
	void Start () {
        m_generator = GetComponent<RoomGenerator> ();
        m_generatedRooms = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.G)) {
            if (m_room != null) {
                Destroy (m_room);
            }
            m_room = m_generator.GenerateRoom ("Room " + m_generatedRooms, Vector2.zero);
            m_generatedRooms++;
        }
	}
}

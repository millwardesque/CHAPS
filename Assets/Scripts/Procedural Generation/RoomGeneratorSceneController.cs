﻿using UnityEngine;
using System.Collections;

public class RoomGeneratorSceneController : MonoBehaviour {
    LevelManager m_generator;
    int m_generatedRooms = 0;
    int m_generatedLayouts = 0;
	
    // Use this for initialization
	void Start () {
        m_generator = GetComponent<LevelManager> ();
        m_generatedRooms = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.G)) {
            m_generator.GenerateRooms ("Room " + m_generatedRooms, 1);
            m_generatedRooms++;
        }

        if (Input.GetKeyDown(KeyCode.L)) {
            m_generator.GenerateRooms ("Layout " + m_generatedLayouts);
            m_generatedLayouts++;
        }
	}
}

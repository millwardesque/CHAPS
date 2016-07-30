﻿using UnityEngine;
using System.Collections;

public class SafeRoomEntrance : MonoBehaviour {
    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player") {
            OnSafeRoomEnter ();
        }
    }

    void OnSafeRoomEnter() {
        GetComponent<Collider2D> ().isTrigger = false;
        Debug.Log ("Entered the safe room.");
    }
}
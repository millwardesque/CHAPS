﻿using UnityEngine;
using System.Collections;

public class SafeRoomExit : MonoBehaviour {
    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player" && col.bounds.min.x >= GetComponent<Collider2D>().bounds.max.x) {
            OnSafeRoomExit ();
        }
    }

    void OnSafeRoomExit() {
        Debug.Log ("Exited the safe room.");
        GetComponent<Collider2D> ().isTrigger = false;
    }
}

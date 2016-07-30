using UnityEngine;
using System.Collections;

public class SafeRoomEntrance : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            OnSafeRoomEnter ();
        }
    }

    void OnSafeRoomEnter() {
        Debug.Log ("Entered the safe room.");
    }
}

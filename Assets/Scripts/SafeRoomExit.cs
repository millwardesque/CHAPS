using UnityEngine;
using System.Collections;

public class SafeRoomExit : MonoBehaviour {
    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player") {
            OnSafeRoomExit ();
        }
    }

    void OnSafeRoomExit() {
        Debug.Log ("Exited the safe room.");
    }
}

﻿using UnityEngine;
using System.Collections;

public class SafeRoomEntrance : MonoBehaviour {
    public AudioClip doorCloseSound;

    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player") {
            OnSafeRoomEnter ();
        }
    }

    void OnSafeRoomEnter() {
        GetComponent<Collider2D> ().isTrigger = false;
        GetComponent<Collider2D> ().usedByEffector = true;
        GameManager.Instance.AudioManager.PlayOneShot (doorCloseSound);
        Debug.Log ("Entered the safe room.");
    }
}

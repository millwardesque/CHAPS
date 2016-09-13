using UnityEngine;
using System.Collections;

public class SafeRoomExit : MonoBehaviour {
    public AudioClip doorCloseSound;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player" || col.tag == "Enemy") {
            GetComponent<Animator> ().SetFloat ("Open Door", 1f);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player" && col.bounds.min.x >= GetComponent<Collider2D>().bounds.max.x) {            
            OnSafeRoomExit ();
        }
    }

    void OnSafeRoomExit() {
        GameManager.Instance.Messenger.SendMessage (this, "SafeRoomExit", this);
        GameManager.Instance.Audio.PlaySFX (doorCloseSound);
    }
}

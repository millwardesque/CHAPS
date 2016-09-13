using UnityEngine;
using System.Collections;

public class SafeRoomEntrance : MonoBehaviour {
    public AudioClip doorCloseSound;

    void Awake() {
        GameManager.Instance.Messenger.AddListener ("SafeRoomExit", OnSafeRoomExit);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player" || col.tag == "Enemy") {
            GetComponent<Animator> ().SetFloat ("Open Door", 1f);
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player" || col.tag == "Enemy") {
            GetComponent<Animator> ().SetFloat ("Open Door", -1f);
        }

        if (col.tag == "Player") {
            OnSafeRoomEnter ();
        }
    }

    void OnSafeRoomEnter() {
        GetComponent<Collider2D> ().isTrigger = false;
        GetComponent<Collider2D> ().usedByEffector = true;
        GameManager.Instance.Audio.PlaySFX (doorCloseSound);
        GameManager.Instance.Audio.SetBGM (null);

        GameManager.Instance.Messenger.SendMessage (this, "SafeRoomEnter", this);
    }

    void OnSafeRoomExit(Message message) {
        GetComponent<Collider2D> ().isTrigger = true;
        GetComponent<Collider2D> ().usedByEffector = false;
    }
}

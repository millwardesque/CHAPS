using UnityEngine;
using System.Collections;

public class SafeRoomExit : MonoBehaviour {
    public AudioClip doorCloseSound;

    void OnTriggerExit2D(Collider2D col) {
        if (col.tag == "Player" && col.bounds.min.x >= GetComponent<Collider2D>().bounds.max.x) {
            OnSafeRoomExit ();
        }
    }

    void OnSafeRoomExit() {
        Debug.Log ("Exited the safe room.");
        GetComponent<Collider2D> ().isTrigger = false;
        GetComponent<Collider2D> ().usedByEffector = true;
        GameManager.Instance.Player.GetComponent<AudioSource> ().PlayOneShot (doorCloseSound);
    }
}

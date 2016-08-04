using UnityEngine;
using System.Collections;

public class RoomSpawnTrigger : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D col) {
        GameManager.Instance.Messenger.SendMessage (this, "RoomSpawnTrigger", this);
        GetComponent<Collider2D> ().enabled = false;
    }
}

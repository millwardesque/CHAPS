using UnityEngine;
using System.Collections;

public class NewZoneTrigger : MonoBehaviour {
    public LevelZone zone;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            GameManager.Instance.Messenger.SendMessage(this, "EnteringZone", zone);
            GameManager.Instance.Audio.SetBGM(zone.backgroundMusic);
            GetComponent<Collider2D>().enabled = false;
        }
    }
}

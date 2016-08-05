using UnityEngine;
using System.Collections;

public class UploadTerminal : MonoBehaviour {
    public AudioClip uploadSound;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            UseTerminal();
            GetComponent<Collider2D> ().enabled = false;
        }
    }

    void UseTerminal() {
        PlayerController player = GameManager.Instance.Player.GetComponent<PlayerController>();
        if (player.IntelPointsCollected > 0) {
            GameManager.Instance.TotalIntelCollected += player.IntelPointsCollected;
            player.IntelPointsCollected = 0;

            GameManager.Instance.AudioManager.PlayOneShot (uploadSound);

            // @TODO Show player using terminal
            // @TODO Show terminal uploading
        }
    }
}

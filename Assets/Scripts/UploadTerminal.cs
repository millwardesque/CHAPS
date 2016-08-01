using UnityEngine;
using System.Collections;

public class UploadTerminal : MonoBehaviour {
    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            UseTerminal();
        }
    }

    void UseTerminal() {
        PlayerController player = GameManager.Instance.Player.GetComponent<PlayerController>();
        if (player.IntelPointsCollected > 0) {
            GameManager.Instance.TotalIntelCollected += player.IntelPointsCollected;
            player.IntelPointsCollected = 0;

            // @TODO Show player using terminal
            // @TODO Show terminal uploading
        }
    }
}

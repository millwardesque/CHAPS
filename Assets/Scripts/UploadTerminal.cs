using UnityEngine;
using System.Collections;

public class UploadTerminal : MonoBehaviour {
    bool hasBeenUsed = false;

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            if (!hasBeenUsed) {
                UseTerminal();
            }
        }
    }

    void UseTerminal() {
        // @TODO Show player using terminal
        // @TODO Show terminal uploading
    }
}

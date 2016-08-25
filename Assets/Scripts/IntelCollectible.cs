using UnityEngine;
using System.Collections;

public class IntelCollectible : MonoBehaviour {
    public int intelPoints = 1;
    public AudioClip collectionNoise;

    public void Collect() {
        GameManager.Instance.Audio.PlaySFX (collectionNoise);
        Destroy (gameObject);
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.tag == "Player") {
            Collect ();
            col.GetComponent<PlayerController> ().IntelPointsCollected += intelPoints;
            return;
        }
    }
}

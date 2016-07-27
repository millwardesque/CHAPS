using UnityEngine;
using System.Collections;

public class Vaultable : MonoBehaviour
{
    public virtual void OnVault() { Debug.Log (string.Format("{0}: Vaulted", name)); }

    void OnCollisionExit2D(Collision2D col) {
        if (col.collider.tag == "Player") {
            OnVault ();
        }
    }
}


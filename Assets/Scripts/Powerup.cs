using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour {
    [Range (0f, 60f)]
    public float duration = 5f;

    protected float m_elapsed = 0f;
    protected bool m_isTriggered = false;

    void Update() {
        if (m_isTriggered && m_elapsed <= duration) {
            m_elapsed += Time.deltaTime;

            if (m_elapsed > duration) {
                OnExpired ();
            }
        }
    }

    public void Trigger() {
        m_isTriggered = true;
        OnTriggered ();
    }

    protected virtual void OnTriggered() {
        Debug.Log ("Power up " + name + " triggered");
        GetComponent<SpriteRenderer> ().enabled = false;
        GetComponent<Collider2D> ().enabled = false;
    }

    protected virtual void OnExpired() {
        Debug.Log ("Power up " + name + " expired");
        Destroy (gameObject);
    }
}

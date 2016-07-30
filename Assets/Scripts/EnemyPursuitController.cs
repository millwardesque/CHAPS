using UnityEngine;
using System.Collections;

public class EnemyPursuitController : InputController {
    GameObject m_player;

    [Range(0, 40f)]
    public float playerAlertRadius = 1f;

    void Start() {
        m_player = GameManager.Instance.Player.gameObject;
    }
    public override bool GetButtonUp (string buttonName) {
        return false;
    }

    public override bool GetButtonDown (string buttonName) {
        return false;
    }

    public override bool GetButton(string buttonName) {
        return false;
    }

    public override float GetAxis (string axisName) {
        if (axisName == "Move Horizontal") {

            // Check whether the player is in our sight range.
            Vector2 toPlayer = (m_player.transform.position - transform.position);
            if (toPlayer.magnitude > playerAlertRadius) {
                return 0f;
            }

            if (toPlayer.x > Mathf.Epsilon) {
                return 1f;
            }
            else if (toPlayer.x < -Mathf.Epsilon) {
                return -1f;
            }
            else {
                return 0f;
            }
        }
        else {
            return 0f;
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = new Color32(255, 0, 0, 64);
        Gizmos.DrawSphere (transform.position, playerAlertRadius);
    }
}

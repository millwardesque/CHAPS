using UnityEngine;
using System.Collections;

public class EnemyPursuitController : InputController {
    GameObject m_player;

    void Start() {
        m_player = GameManager.Instance.Player.gameObject;
        Debug.Log (m_player.name);
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
        if (axisName == "Horizontal") {
            Vector2 directionToPlayer = (m_player.transform.position - transform.position);
            Debug.Log (directionToPlayer);
            if (directionToPlayer.x > Mathf.Epsilon) {
                return 1f;
            }
            else if (directionToPlayer.x < -Mathf.Epsilon) {
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
}

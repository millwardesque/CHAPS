using UnityEngine;
using System.Collections;

public class EnemyPursuitController : InputController {
    GameObject m_player;

    [Range(0, 40f)]
    public float playerAlertRadius = 1f;

    [Range(0, 2f)]
    public float jumpCheckDistance = 1f;

    bool m_pressedJump = true;  // Used to guarantee that the jump button is only held down for one frame.

    void Start() {
        m_player = GameManager.Instance.Player.gameObject;
    }
    public override bool GetButtonUp(string buttonName) {
        if (m_pressedJump == true) {
            m_pressedJump = false;  // Reset the pressed jump flag so it doesn't keep firing.
            return true;
        }
        else {
            return false;
        }
    }

    public override bool GetButtonDown(string buttonName) {
        if (m_player == null) { // @TODO This should eventually be detected and fixed. This check is a bandaid.
            return false;
        }

        if (buttonName == "Jump") {
            Vector2 toPlayer = (m_player.transform.position - transform.position);
            if (toPlayer.magnitude <= playerAlertRadius) {
                // Check for a foot collision first.
                RaycastHit2D hit = Physics2D.Raycast(GetJumpDetectionFootPosition(), new Vector2(toPlayer.normalized.x, 0f), jumpCheckDistance, LayerMask.GetMask("Default", "Environment"));
                if (hit.collider != null && hit.collider.GetComponent<SafeRoomEntrance>() == null) {    // Make sure the enemy doesn't jump into the closed safe room door.
                    m_pressedJump = true;
                    return true;
                }
                else {  // If not found, check for a head collision.
                    hit = Physics2D.Raycast(GetJumpDetectionHeadPosition(), new Vector2(toPlayer.normalized.x, 0f), jumpCheckDistance, LayerMask.GetMask("Default", "Environment"));
                    if (hit.collider != null && hit.collider.GetComponent<SafeRoomEntrance>() == null) {    // Make sure the enemy doesn't jump into the closed safe room door.
                        m_pressedJump = true;
                        return true;
                    }
                    else {  // If not found, check for a body collision.
                        hit = Physics2D.Raycast(GetJumpDetectionBodyPosition(), new Vector2(toPlayer.normalized.x, 0f), jumpCheckDistance, LayerMask.GetMask("Default", "Environment"));
                        if (hit.collider != null && hit.collider.GetComponent<SafeRoomEntrance>() == null) {    // Make sure the enemy doesn't jump into the closed safe room door.
                            m_pressedJump = true;
                            return true;
                        }
                    }
                }
            }

        }
        return false;
    }

    public override bool GetButton(string buttonName) {
        return false;
    }

    public override float GetAxis(string axisName) {
        if (axisName == "Move Horizontal") {
            if (m_player == null) { // @TODO This should eventually be detected and fixed. This check is a bandaid.
                return 0f;
            }

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
        Gizmos.DrawSphere(transform.position, playerAlertRadius);

        Vector2 toPlayer = (m_player.transform.position - transform.position);

        Gizmos.color = new Color(255, 0, 0);
        Vector2 start;
        start = GetJumpDetectionHeadPosition();
        Gizmos.DrawLine(start, start + new Vector2(toPlayer.normalized.x * jumpCheckDistance, 0f));

        Gizmos.color = new Color(255, 0, 0);
        start = GetJumpDetectionBodyPosition();
        Gizmos.DrawLine(start, start + new Vector2(toPlayer.normalized.x * jumpCheckDistance, 0f));

        Gizmos.color = new Color(255, 0, 0);
        start = GetJumpDetectionFootPosition();
        Gizmos.DrawLine(start, start + new Vector2(toPlayer.normalized.x * jumpCheckDistance, 0f));

    }

    Vector2 GetJumpDetectionHeadPosition() {
        return (Vector2)(transform.position) + new Vector2(0f, 0.8f);
    }

    Vector2 GetJumpDetectionFootPosition() {
        return (Vector2)(GetComponent<PlatformerMotor>().footPosition.transform.position) + new Vector2(0f, 0.1f);
    }

    Vector2 GetJumpDetectionBodyPosition() {
        return (Vector2)transform.position;
    }
}

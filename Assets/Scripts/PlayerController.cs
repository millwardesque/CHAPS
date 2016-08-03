using UnityEngine;
using Rewired;

public class PlayerController : InputController {
    PlatformerMotor m_motor;
    Player m_rewiredPlayer;    // Rewired player.
    AudioSource m_audioSource;

    public AudioClip jumpSound;
    public AudioClip multiJumpSound;

    int m_intelPointsCollected = 0;
    public int IntelPointsCollected {
        get { return m_intelPointsCollected; }
        set {
            m_intelPointsCollected = value;
            GameManager.Instance.Messenger.SendMessage (this, "IntelPointsChanged", m_intelPointsCollected);
        }
    }

    void Awake() {
        m_rewiredPlayer = ReInput.players.GetPlayer(0);
        m_motor = GetComponent<PlatformerMotor> ();
        m_audioSource = GetComponent<AudioSource> ();
        GameManager.Instance.Messenger.AddListener("PlatformerJumped", OnPlatformerJumped);
        GameManager.Instance.Messenger.AddListener("PlatformerMultiJumped", OnPlatformerMultiJumped);
    }

    public override bool GetButtonUp (string buttonName) {
        return m_rewiredPlayer.GetButtonUp (buttonName);
    }

    public override bool GetButtonDown (string buttonName) {
        return m_rewiredPlayer.GetButtonDown (buttonName);
    }

    public override bool GetButton(string buttonName) {
        return m_rewiredPlayer.GetButton (buttonName);
    }

    public override float GetAxis (string axisName) {
        return m_rewiredPlayer.GetAxis (axisName);
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.tag == "Enemy") {
            bool isHeadStomp = false;
            foreach (ContactPoint2D contact in col.contacts) {
                if (contact.normal.y > 0f) {
                    isHeadStomp = true;
                    break;
                }
            }

            if (isHeadStomp) {
                col.collider.GetComponent<EnemyHordeMember> ().OnStomped ();
                m_motor.ReplaceState (new PlatformerMotorStateJumping(m_motor, m_motor.CurrentState, m_motor.maxBounceDuration));
            }
            else {
                GameManager.Instance.Messenger.SendMessage(new Message(this, "GameOver"));    
            }

            return;
        }

        if (col.collider.GetComponent<IntelCollectible>()) {
            IntelPointsCollected += col.collider.GetComponent<IntelCollectible> ().intelPoints;
            Destroy (col.collider.gameObject);
            return;
        }
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.GetComponent<Powerup>()) {
            col.GetComponent<Powerup> ().Trigger ();
            return;
        }
    }

    void OnPlatformerJumped(Message message) {
        PlatformerMotor jumper = (PlatformerMotor)message.data;
        if (jumper == m_motor) {
            m_audioSource.PlayOneShot (jumpSound);
        }
    }

    void OnPlatformerMultiJumped(Message message) {
        PlatformerMotor jumper = (PlatformerMotor)message.data;
        if (jumper == m_motor) {
            m_audioSource.PlayOneShot (multiJumpSound);
        }
    }
}

using System.Collections;
using UnityEngine;
using Rewired;

public class PlayerController : InputController {
    PlatformerMotor m_motor;
    Player m_rewiredPlayer;    // Rewired player.

    public Collider2D enemyProximitySensor;
    public bool autorun;
    public AudioClip runSound;
    public AudioClip jumpSound;
    public AudioClip multiJumpSound;
    public AudioClip deathSound;
    public AudioClip headStompSound;

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
        GameManager.Instance.Messenger.AddListener("StartedRunning", OnStartedRunning);
        GameManager.Instance.Messenger.AddListener("StoppedRunning", OnStoppedRunning);
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
        if (autorun && axisName == "Move Horizontal") { 
            return 1f;
        }
        else {
            return m_rewiredPlayer.GetAxis (axisName);    
        }
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
                GameManager.Instance.Audio.PlaySFX  (headStompSound);
                m_motor.ReplaceState (new PlatformerMotorStateJumping(m_motor, m_motor.CurrentState, m_motor.maxBounceDuration, true));
            }
            else {
                m_motor.ReplaceState (new PlatformerMotorStateDead (m_motor, m_motor.CurrentState));

                GameManager.Instance.Audio.PlaySFX(deathSound);
                GameManager.Instance.Audio.StopBank("player_movement");

                StartCoroutine ("WaitForPlayerDeathAnimation");
            }

            return;
        }

        if (col.collider.GetComponent<IntelCollectible>()) {
            GameManager.Instance.Audio.PlaySFX  (col.collider.GetComponent <IntelCollectible>().collectionNoise);
            IntelPointsCollected += col.collider.GetComponent<IntelCollectible> ().intelPoints;
            Destroy (col.collider.gameObject);
            return;
        }
    }

    IEnumerator WaitForPlayerDeathAnimation() {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.Messenger.SendMessage(new Message(this, "GameOver"));
    }

    void OnTriggerEnter2D(Collider2D col) {
        if (col.GetComponent<Powerup>()) {
            GameManager.Instance.Audio.PlaySFX (col.GetComponent <Powerup>().collectionNoise);
            col.GetComponent<Powerup> ().Trigger ();
            return;
        }

        if (col.GetComponent<EnemyHordeMember>()) {
            GameManager.Instance.Messenger.SendMessage (this, "PlayerCloseToEnemy", col.gameObject);
            return;
        }
    }

    void OnTriggerExit2D(Collider2D col) {
        if (col.GetComponent<EnemyHordeMember>()) {
            GameManager.Instance.Messenger.SendMessage (this, "PlayerFarFromEnemy", col.gameObject);
            return;
        }
    }

    void OnStartedRunning(Message message) {
        PlatformerMotor runner = (PlatformerMotor)message.data;
        if (runner == m_motor) {
            GameManager.Instance.Audio.PlayClipInBank("player_movement", runSound, true);
        }
    }

    void OnStoppedRunning(Message message) {
        PlatformerMotor runner = (PlatformerMotor)message.data;
        if (runner == m_motor) {
            GameManager.Instance.Audio.StopBank("player_movement");
        }
    }

    void OnPlatformerJumped(Message message) {
        PlatformerMotor jumper = (PlatformerMotor)message.data;
        if (jumper == m_motor) {
            GameManager.Instance.Audio.PlayClipInBank("player_movement", jumpSound, false);
        }
    }

    void OnPlatformerMultiJumped(Message message) {
        PlatformerMotor jumper = (PlatformerMotor)message.data;
        if (jumper == m_motor) {
            GameManager.Instance.Audio.PlayClipInBank("player_movement", multiJumpSound, false);
        }
    }
}

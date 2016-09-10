using UnityEngine;
using System.Collections;

public class EnemyHordeMember : MonoBehaviour {
    public float hordeJoinDistanceFromPlayer = 2f;
    public float stunDuration = 1f;

    EnemyHorde m_horde = null;
    public EnemyHorde Horde {
        get { return m_horde; }
        set {
            m_horde = value;
            if (value != null) {
                SetMaxSpeed(m_horde.MaxHordeSpeed);
            }
        }
    }

    PlatformerMotor m_motor;

    void Awake () {
        GameManager.Instance.Messenger.AddListener("HordeSpeedChanged", OnHordeSpeedChange);
        m_motor = GetComponent<PlatformerMotor>();
    }
	
    void Update() {
        if (m_motor.CurrentState.GetType() != typeof(PlatformerMotorStateStunned) && GameManager.Instance.Player.transform.position.x - transform.position.x >= hordeJoinDistanceFromPlayer && m_horde == null) {

            // Note: This will force the layer to Enemy Horde Member. See OnStomped for why this is important to note.
            EnemyHorde horde = FindObjectOfType<EnemyHorde>();
            if (horde != null) {
                horde.AddEnemyToHorde(this);
            }
        }
    }

    void OnHordeSpeedChange (Message message) {
        if (message.sender == m_horde) {
            SetMaxSpeed(m_horde.MaxHordeSpeed);
        }
    }

    void SetMaxSpeed(float maxSpeed) {
        GetComponent<PlatformerMotor>().maxSpeed = maxSpeed;
    }

    public void OnStomped() {
        m_motor.ReplaceState(new PlatformerMotorStateStunned(m_motor, m_motor.CurrentState, stunDuration));

        // Set the layer to default so other enemies will collide with / try to avoid the stunned enemy.
        gameObject.layer = LayerMask.NameToLayer("Default");
    }
}

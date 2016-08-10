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

	void Awake () {
        GameManager.Instance.Messenger.AddListener("HordeSpeedChanged", OnHordeSpeedChange);
	}
	
    void Update() {
        if (GameManager.Instance.Player.transform.position.x - transform.position.x >= hordeJoinDistanceFromPlayer && m_horde == null) {
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
        PlatformerMotor motor = GetComponent<PlatformerMotor> ();
        motor.ReplaceState(new PlatformerMotorStateStunned(motor, motor.CurrentState, stunDuration));

    }
}

using UnityEngine;
using System.Collections.Generic;

public class EnemyHorde : MonoBehaviour {
    List<EnemyHordeMember> m_members;

    [Range (0f, 5f)]
    public float playerSpeedOffset = 1f;

    [Range (0f, 2f)]
    public float speedIncrementPerMember = 0.1f;

    float m_maxHordeSpeed = 1f;
    public float MaxHordeSpeed {
        get { return m_maxHordeSpeed; }
        set {
            m_maxHordeSpeed = Mathf.Max(0f, value);
            GameManager.Instance.Messenger.SendMessage(new Message(this, "HordeSpeedChanged", m_maxHordeSpeed));
        }
    }


    void Awake() {
        m_members = new List<EnemyHordeMember>();
    }

    void Start() {
        MaxHordeSpeed = (GameManager.Instance.Player.maxSpeed - playerSpeedOffset);
    }

    public void AddEnemyToHorde(EnemyHordeMember enemy) {
        if (enemy != null) {
            enemy.transform.SetParent(transform.parent, true);
            enemy.gameObject.layer = LayerMask.NameToLayer("Enemy Horde");
            enemy.GetComponentInChildren<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Enemy Horde");
            if (enemy.GetComponent<EnemyPursuitController>()) {
                enemy.GetComponent<EnemyPursuitController>().playerAlertRadius = 100f;  // Don't stop chasing the player.
            }
            enemy.Horde = this;
            m_members.Add(enemy);
            MaxHordeSpeed = (GameManager.Instance.Player.maxSpeed - playerSpeedOffset) + speedIncrementPerMember * m_members.Count;
        }        
    }

    public void RemoveEnemyFromHorde(EnemyHordeMember enemy) {
        if (enemy != null && enemy.Horde == this) {
            enemy.Horde = null;
            m_members.Remove (enemy);
            MaxHordeSpeed -= speedIncrementPerMember;
        }
    }
}

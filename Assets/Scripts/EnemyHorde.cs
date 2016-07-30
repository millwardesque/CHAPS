using UnityEngine;
using System.Collections.Generic;

public class EnemyHorde : MonoBehaviour {
    List<EnemyHordeMember> m_members;

    [Range (0f, 20f)]
    public float startMaxHordeSpeed = 1f;

    [Range (0f, 5f)]
    public float speedIncrementPerMember = 1f;

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
        MaxHordeSpeed = startMaxHordeSpeed;
    }

    public void AddEnemyToHorde(EnemyHordeMember enemy) {
        if (enemy != null) {
            enemy.transform.SetParent(transform.parent, true);
            enemy.gameObject.layer = LayerMask.NameToLayer("Enemy Horde");
            enemy.GetComponentInChildren<SpriteRenderer>().sortingLayerID = SortingLayer.NameToID("Enemy Horde");
            enemy.GetComponent<Rigidbody2D>().gravityScale = 0f;
            enemy.Horde = this;
            m_members.Add(enemy);
            MaxHordeSpeed += speedIncrementPerMember;
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

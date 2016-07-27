using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    MessageManager m_messenger = null;
    public MessageManager Messenger {
        get { return m_messenger; }
    }

    PlatformerMotor m_player = null;
    public PlatformerMotor Player {
        get { return m_player; }
    }

    public static GameManager Instance = null;

    void Awake() {
        if (null == Instance) {
            Instance = this;
            m_messenger = new MessageManager ();
        } else {
            Destroy (gameObject);
        }
    }

    void Start() {
        m_player = FindObjectOfType<PlatformerMotor> ();
    }
    
}

using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
    MessageManager m_messenger = null;
    public MessageManager Messenger {
        get { return m_messenger; }
    }

    public static GameManager Instance = null;

    void Awake() {
        if (null == Instance) {
            Instance = this;
            m_messenger = new MessageManager ();
        }
        else {
            Destroy (gameObject);
        }
    }
}

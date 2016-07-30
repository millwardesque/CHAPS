using UnityEngine;
using Rewired;

public class PlayerController : InputController {
    Player m_player;    // Rewired player.

    int m_intelPointsCollected = 0;
    public int IntelPointsCollected {
        get { return m_intelPointsCollected; }
        set {
            m_intelPointsCollected = value;
            GameManager.Instance.Messenger.SendMessage (this, "IntelPointsChanged", m_intelPointsCollected);
        }
    }

    void Awake() {
        m_player = ReInput.players.GetPlayer(0);
    }

    public override bool GetButtonUp (string buttonName) {
        return m_player.GetButtonUp (buttonName);
    }

    public override bool GetButtonDown (string buttonName) {
        return m_player.GetButtonDown (buttonName);
    }

    public override bool GetButton(string buttonName) {
        return m_player.GetButton (buttonName);
    }

    public override float GetAxis (string axisName) {
        return m_player.GetAxis (axisName);
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (col.collider.GetComponent<IntelCollectible>()) {
            IntelPointsCollected += col.collider.GetComponent<IntelCollectible> ().intelPoints;
            Destroy (col.collider.gameObject);   
        }
    }
}

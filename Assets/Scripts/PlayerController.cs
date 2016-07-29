using UnityEngine;
using Rewired;

public class PlayerController : InputController {
    Player m_player;    // Rewired player.

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
}

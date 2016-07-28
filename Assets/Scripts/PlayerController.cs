using UnityEngine;
using System.Collections;

public class PlayerController : InputController {
    public override bool GetButtonUp (string buttonName) {
        return Input.GetButtonUp (buttonName);
    }

    public override bool GetButtonDown (string buttonName) {
        return Input.GetButtonDown (buttonName);
    }

    public override bool GetButton(string buttonName) {
        return Input.GetButton (buttonName);
    }

    public override float GetAxis (string axisName) {
        return Input.GetAxis (axisName);
    }
}

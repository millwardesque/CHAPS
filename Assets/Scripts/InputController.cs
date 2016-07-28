using UnityEngine;
using System.Collections;

public abstract class InputController : MonoBehaviour {
    public abstract bool GetButtonUp (string buttonName);
    public abstract bool GetButtonDown (string buttonName);
    public abstract bool GetButton(string buttonName);
    public abstract float GetAxis (string axisName);
}

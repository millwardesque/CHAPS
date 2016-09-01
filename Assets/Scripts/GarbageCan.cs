using UnityEngine;
using System.Collections;

public class GarbageCan : MonoBehaviour {

    public void OnJumpOff() {
        Debug.Log("Player jumping off");
        GetComponentInChildren<Animator>().SetTrigger("Tip Over");
    }
}

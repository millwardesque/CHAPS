using UnityEngine;
using System.Collections;

public class MultiJumpUpgrade : Powerup {
    public int numTotalJumps = 2;

    void Start() {
        duration = -1f;
    }

    protected override void OnTriggered() {
        base.OnTriggered();

        GameManager.Instance.Player.maxJumps = numTotalJumps;
    }
}

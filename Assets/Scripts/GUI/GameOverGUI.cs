using UnityEngine;
using System.Collections;

public class GameOverGUI : MonoBehaviour {
    public void OnRestartGameClick() {
        GameManager.Instance.RestartGame();
        Destroy(gameObject);
    }
}

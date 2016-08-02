using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class GameOverGUI : MonoBehaviour {
    void Start() {
        Button restartButton = GetComponentInChildren<Button> ();
        EventSystem.current.SetSelectedGameObject (restartButton.gameObject);        
    }

    public void OnRestartGameClick() {
        GameManager.Instance.RestartGame();
        Destroy(gameObject);
    }
}

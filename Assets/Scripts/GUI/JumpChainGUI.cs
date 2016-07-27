using UnityEngine;
using UnityEngine.UI;

public class JumpChainGUI : MonoBehaviour {
    public Text valueLabel;
	
    void Start() {
        GameManager.Instance.Messenger.AddListener ("JumpChainChanged", OnJumpChainChanged);
        UpdateJumpValue (0, GameManager.Instance.Player.maxChainJumps);
    }

    void OnJumpChainChanged(Message message) {
        int newValue = (int)message.data;
        UpdateJumpValue (newValue, GameManager.Instance.Player.maxChainJumps);
    }

    void UpdateJumpValue(int newValue, int maxValue) {
        valueLabel.text = string.Format("{0} / {1}", newValue, maxValue);
    }
}

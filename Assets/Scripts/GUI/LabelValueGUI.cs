using UnityEngine;
using UnityEngine.UI;

public class LabelValueGUI : MonoBehaviour {
    public Text valueLabel;
    public string messageName;
    public string initialValue;

    void Awake() {
        GameManager.Instance.Messenger.AddListener(messageName, OnValueChanged);
        UpdateValue(initialValue);
    }

    void OnValueChanged(Message message) {
        UpdateValue (message.data.ToString ());
    }

    void UpdateValue(string newValue) {
        valueLabel.text = newValue;
    }
}

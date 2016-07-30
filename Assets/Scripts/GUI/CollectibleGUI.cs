using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CollectibleGUI : MonoBehaviour {
    public Text valueLabel;

    void Start() {
        GameManager.Instance.Messenger.AddListener ("IntelPointsChanged", OnValueChanged);
        UpdateValue (0);
    }

    void OnValueChanged(Message message) {
        int newValue = (int)message.data;
        UpdateValue (newValue);
    }

    void UpdateValue(int newValue) {
        valueLabel.text = newValue.ToString ();
    }
}

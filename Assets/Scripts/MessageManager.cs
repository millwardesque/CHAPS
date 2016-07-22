using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Message {
    public MonoBehaviour sender;
    public string name;
    public object data;

    public Message(MonoBehaviour sender, string name, object data = null) {
        this.sender = sender;
        this.name = name;
        this.data = data;
    }
}

public delegate void MessageReceiver(Message message);

public class MessageManager {
    Dictionary<string, List<MessageReceiver>> listeners = new Dictionary<string, List<MessageReceiver>>();

    public void AddListener(string messageName, MessageReceiver listener) {
        if (!listeners.ContainsKey(messageName)) {
            listeners[messageName] = new List<MessageReceiver>();
        }
        listeners[messageName].Add(listener);
    }

    public void SendMessage(Message message) {
        if (listeners.ContainsKey(message.name)) {
            List<MessageReceiver> recipients = listeners[message.name];
            for (int i = 0; i < recipients.Count; ++i) {
                recipients[i].Invoke(message);
            }
        }
    }

    public void SendMessage(MonoBehaviour sender, string name, object data = null) {
        Message message = new Message(sender, name, data);
        SendMessage(message);
    }
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(Collider2D))]
public class PlatformerMotor : MonoBehaviour {
    public float runSpeed = 1f;
    public float jumpControlSpeed = 1f;
    public float jumpForce = 10f;
    public int maxJumps = 2;
    public float fallControlSpeed = 1f;
    public float timeUntilFalling = 0.2f;   // Time in the air until the player is considered to be falling.

    int m_surfaceCollisions = 0;
    public int SurfaceCollisionCount {
        get { return m_surfaceCollisions; }
    }

    float m_timeFalling = 0f;
    public float TimeFalling {
        get { return m_timeFalling; }
    }

    Rigidbody2D m_rb;
    public Rigidbody2D RB {
        get { return m_rb; }
    }

    Stack<PlatformerMotorState> m_stateStack = null;
    public PlatformerMotorState CurrentState {
        get { return (m_stateStack != null ? m_stateStack.Peek () : null); }
    }

    void Awake() {
        m_stateStack = new Stack<PlatformerMotorState> ();
    }

    void Start() {
        m_rb = GetComponent<Rigidbody2D> ();
        m_surfaceCollisions = 0;
        PushState (new PlatformerMotorStateIdle (this, null));
    }

    void FixedUpdate() { 
        if (CurrentState != null) {
            CurrentState.FixedUpdate ();
        }
    }

    void Update() { 
        if (CurrentState != null) {
            CurrentState.HandleInput ();
        }
    }

    public void PushState(PlatformerMotorState newState) {
        if (m_stateStack.Count > 0) { 
            m_stateStack.Peek().Exit();
        }

        m_stateStack.Push(newState);
        newState.Enter ();
    }

    public PlatformerMotorState PopState() {
        if (m_stateStack.Count > 0) {
            m_stateStack.Peek ().Exit ();
        }
        return m_stateStack.Pop ();
    }

    public void ReplaceState(PlatformerMotorState newState) {
        PopState ();
        PushState (newState);
    }

    void OnCollisionEnter2D(Collision2D col) {
        Vector2 averageNormal = new Vector2();
        for (int i = 0; i < col.contacts.Length; ++i) {
            averageNormal += col.contacts[i].normal;
        }
        averageNormal.Normalize();

        if (averageNormal.y > 0f) {
            m_surfaceCollisions++;

            if (m_surfaceCollisions == 1 && CurrentState != null) {
                CurrentState.OnHasLanded(); // @TODO Replace with decoupled message
            }
        }
    }

    void OnCollisionExit2D(Collision2D col) {
        Vector2 averageNormal = new Vector2();
        for (int i = 0; i < col.contacts.Length; ++i) {
            averageNormal += col.contacts[i].normal;
        }
        averageNormal.Normalize();

        if (averageNormal.y > 0f) {
            m_surfaceCollisions--;
        }
    }
}

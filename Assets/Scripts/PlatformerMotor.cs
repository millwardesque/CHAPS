﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(Collider2D))]
public class PlatformerMotor : MonoBehaviour {
    [HideInInspector]
    public string groundedCollisionObject = "";

    [Header("Walking")]
    [Range (0f, 50f)]
    public float maxSpeed = 20f;

    [Range (0f, 1f)]
    public float velocityFrictionFactor = 0f;

    [Range (0f, 1f)]
    public float accelerationRate = 1f;

    [Header("Airborn")]
    [Range (0f, 50f)]
    public float airControlSpeed = 1f;

    [Range (0f, 10f)]
    public float jumpForce = 10f;

    [Range (0f, 5f)]
    public float minJumpHeight = 0f;

    [Range (0f, 1f)]
    public float maxJumpDuration = 1f;

    [Range (0, 10)]
    public int maxJumps = 2;

    [Range (0f, 1f)]
    public float timeUntilFalling = 0.2f;   // Time in the air until the player is considered to be falling.

    [Header("General")]
    public LayerMask collisionLayers;

    [Range (0f, 1f)]
    public float maxBounceDuration = 1f;

    public Transform avatarRoot;

    public Transform footPosition;

    Rigidbody2D m_rb;
    public Rigidbody2D RB {
        get { return m_rb; }
    }

    Stack<PlatformerMotorState> m_stateStack = null;
    public PlatformerMotorState CurrentState {
        get { return (m_stateStack != null ? m_stateStack.Peek () : null); }
    }

    InputController m_controller;
    public InputController Controller {
        get { return m_controller; }
    }

    Animator m_animator;
    public Animator AnimationController
    {
        get { return m_animator; }
    }

    void Awake() {
        m_stateStack = new Stack<PlatformerMotorState> ();
        m_animator = GetComponentInChildren<Animator>(true);
    }

    void Start() {
        m_controller = GetComponent<InputController> ();
        Debug.Assert (m_controller != null, string.Format("{0}: PlatformerMotor is missing an InputController subclass on the same component.", name));
        m_rb = GetComponent<Rigidbody2D> ();
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

    public bool IsGrounded() {
        Vector2 foot = footPosition.position;
        Collider2D col = Physics2D.OverlapCircle(foot, 0.2f, collisionLayers);
        groundedCollisionObject = col == null ? "None" : col.name;

        return (col != null);
    }

    void OnCollisionEnter2D(Collision2D col) {
        if (CurrentState != null) {
            CurrentState.OnCollisionEnter2D (col);
        }
    }
}

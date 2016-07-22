using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for all platformer motor states.
/// </summary>
public class PlatformerMotorState {
    protected PlatformerMotor m_owner;

    Vector2 m_requestedMovementDirection;
    protected Vector2 RequestedMovementDirection {
        get { return m_requestedMovementDirection; }
    }

    bool m_hasRequestedJump;

    protected int m_jumpCount = 0;

    protected Vector2 Velocity {
        get { return m_owner.RB.velocity; }
    }

    float m_timeAscending;
    protected float TimeAscending {
        get { return m_timeAscending; }
    }

    float m_timeDescending;
    protected float TimeDescending {
        get { return m_timeDescending; }
    }

    public PlatformerMotorState(PlatformerMotor owner, PlatformerMotorState previousState) {
        m_owner = owner;

        if (previousState != null) {
            m_requestedMovementDirection = previousState.RequestedMovementDirection;
            m_timeDescending = previousState.TimeDescending;
            m_timeAscending = previousState.TimeAscending;
            m_hasRequestedJump = previousState.m_hasRequestedJump;
            m_jumpCount = previousState.m_jumpCount;
        }
    }

    public virtual void Enter() { /* Debug.Log ("Entering " + GetType ().ToString ()); */ }

    public virtual void HandleInput() {
        m_requestedMovementDirection = new Vector2 (Input.GetAxis ("Horizontal"), 0f);

        if (Input.GetButtonDown("Jump")) {
            m_hasRequestedJump = true;
        }
        else {
            m_hasRequestedJump = false;
        }
    }

    public virtual void FixedUpdate() {
        if (m_owner.SurfaceCollisionCount == 0) {
            if (Velocity.y < 0f) {
                m_timeDescending += Time.fixedDeltaTime;
                m_timeAscending = 0f;
            }
            else if (Velocity.y > 0f) {
                m_timeAscending += Time.fixedDeltaTime;
                m_timeDescending = 0f;
            }
        }
    }

    public virtual void Exit() { /* Debug.Log ("Exiting " + GetType ().ToString ()); */ }

    protected bool HasRequestedMovementDirection () {
        return Mathf.Abs (RequestedMovementDirection.magnitude) > Mathf.Epsilon;
    }

    protected bool HasRequestedJump() {
        return m_hasRequestedJump;
    }

    protected bool IsDescending () {
        return (TimeDescending - m_owner.timeUntilFalling) >= Mathf.Epsilon;
    }

    protected bool IsAscending () { 
        return Mathf.Abs (TimeAscending) >= Mathf.Epsilon;
    }

    protected bool CanJump() {
        return (HasRequestedJump() && m_jumpCount < m_owner.maxJumps);
    }

    public void OnHasLanded() {
        m_jumpCount = 0;
        m_timeAscending = 0f;
        m_timeDescending = 0f;
    }

    public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Force) {
        m_owner.RB.AddForce (force, mode);
    }
}

/// <summary>
/// Idle state when player is standing still on the ground.
/// </summary>
public class PlatformerMotorStateIdle : PlatformerMotorState {
    public PlatformerMotorStateIdle(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void HandleInput() { 
        base.HandleInput ();

        if (CanJump()) {  // Jump input takes priority over movement input.
            m_owner.ReplaceState (new PlatformerMotorStateJumping (m_owner, this));
            return;
        }
        else if (IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateFalling(m_owner, this));
            return;
        }
        else if (HasRequestedMovementDirection()) {
            m_owner.ReplaceState (new PlatformerMotorStateWalking (m_owner, this));
            return;
        }
    }
}

/// <summary>
/// State when the actor is walking around on the ground.
/// </summary>
public class PlatformerMotorStateWalking : PlatformerMotorState {
    Vector2 m_requestedMovementDirection;
    public PlatformerMotorStateWalking(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void HandleInput() {
        base.HandleInput();

        if (CanJump()) {
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
    }

    public override void FixedUpdate() {
        base.FixedUpdate ();

        if (HasRequestedMovementDirection ()) {
            AddForce(RequestedMovementDirection * m_owner.runSpeed);
        }

        if (IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateFalling(m_owner, this));
            return;
        }
        else if (Velocity.magnitude < Mathf.Epsilon) {
            m_owner.ReplaceState (new PlatformerMotorStateIdle(m_owner, this));
            return;
        }
    }
}

/// <summary>
/// State when the actor is falling
/// </summary>
public class PlatformerMotorStateFalling : PlatformerMotorState {
    public PlatformerMotorStateFalling(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void HandleInput() {
        base.HandleInput();

        if (CanJump()) {
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
    }

    public override void FixedUpdate() {
        base.FixedUpdate ();

        if (HasRequestedMovementDirection ()) {
            AddForce(RequestedMovementDirection * m_owner.fallControlSpeed);
        }

        if (!IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateLanded(m_owner, this));
            return;
        }
    }
}

/// <summary>
/// State when the actor is falling
/// </summary>
public class PlatformerMotorStateLanded : PlatformerMotorState {
    public PlatformerMotorStateLanded(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void HandleInput() {
        base.HandleInput();

        if (CanJump()) {  // Jump input takes priority over movement input.
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
        else if (IsDescending()) {
            m_owner.ReplaceState(new PlatformerMotorStateFalling(m_owner, this));
            return;
        }
        else if (HasRequestedMovementDirection()) {
            m_owner.ReplaceState(new PlatformerMotorStateWalking(m_owner, this));
            return;
        }
        else {
            m_owner.ReplaceState(new PlatformerMotorStateIdle(m_owner, this));
        }
    }
}

/// <summary>
/// State when the actor is jumping
/// </summary>
public class PlatformerMotorStateJumping : PlatformerMotorState {
    public PlatformerMotorStateJumping(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void Enter() {
        m_jumpCount++;
        AddForce(new Vector2(0f, m_owner.jumpForce), ForceMode2D.Impulse);
    }

    public override void HandleInput() {
        base.HandleInput();

        if (CanJump()) {  // Double-jump if the player is in the air.
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
    }

    public override void FixedUpdate() {
        base.FixedUpdate ();

        if (HasRequestedMovementDirection ()) {
            AddForce (RequestedMovementDirection * m_owner.jumpControlSpeed);
        }

        if (IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateFalling (m_owner, this));
            return;
        }
        if (m_jumpCount == 0) {
            m_owner.ReplaceState (new PlatformerMotorStateLanded (m_owner, this));
        }
    }
}
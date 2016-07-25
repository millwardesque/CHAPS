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
    bool m_isHoldingJump;

    protected int m_jumpCount = 0;
    protected float m_jumpDuration = 0f;

    protected Vector2 Velocity {
        get { return m_owner.RB.velocity; }
        set { m_owner.RB.velocity = value; }
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
            m_isHoldingJump = previousState.m_isHoldingJump;
            m_jumpCount = previousState.m_jumpCount;
        }
    }

    public virtual void Enter() { /* Debug.Log ("Entering " + GetType ().ToString ()); */ }

    public virtual void HandleInput() {
        m_requestedMovementDirection = new Vector2 (Input.GetAxis ("Horizontal"), 0f);

        if (Input.GetButtonDown("Jump")) {
            m_hasRequestedJump = true;
            m_isHoldingJump = true;
        }
        else if (Input.GetButton ("Jump")) {
            m_isHoldingJump = true;
            m_hasRequestedJump = false;
        }
        else if (Input.GetButtonUp ("Jump")) {
            m_isHoldingJump = false;
            m_hasRequestedJump = false;
        }
    }

    public virtual void FixedUpdate() {
        if (!m_owner.IsGrounded()) {
            if (Velocity.y < -Mathf.Epsilon) {
                m_timeDescending += Time.fixedDeltaTime;
                m_timeAscending = 0f;
            }
            else if (Velocity.y > Mathf.Epsilon) {
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

    protected bool IsHoldingJump() {
        return m_isHoldingJump;
    }

    protected bool IsDescending () {
        return !m_owner.IsGrounded() && (TimeDescending - m_owner.timeUntilFalling) >= Mathf.Epsilon;
    }

    protected bool IsAscending () { 
        return !m_owner.IsGrounded() && Mathf.Abs (TimeAscending) >= Mathf.Epsilon;
    }

    protected bool CanJump() {
        return (HasRequestedJump() && m_jumpCount < m_owner.maxJumps);
    }

    public void OnHasLanded() {
        m_jumpCount = 0;
        m_timeAscending = 0f;
        m_timeDescending = 0f;
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
            float maxXVelocity = RequestedMovementDirection.x * m_owner.fallControlSpeed;
            float newXVelocity = m_owner.accelerationRate * maxXVelocity + (1f - m_owner.accelerationRate) * Velocity.x;
            Velocity = new Vector2(newXVelocity, Velocity.y);
        }
        else {
            Velocity = new Vector2 (Velocity.x * (1f - m_owner.velocityFrictionFactor), Velocity.y);
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

    public override void FixedUpdate() {
        base.FixedUpdate ();

        if (HasRequestedMovementDirection ()) {
            float maxXVelocity = RequestedMovementDirection.x * m_owner.maxSpeed;
            float newXVelocity = m_owner.accelerationRate * maxXVelocity + (1f - m_owner.accelerationRate) * Velocity.x;
            Velocity = new Vector2(newXVelocity, Velocity.y);
        }

        if (m_owner.IsGrounded()) {
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

    public override void Enter() {
        base.Enter();
        m_jumpCount = 0;
        m_jumpDuration = 0f;
    }
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

    float m_startingHeight = 0f;
    float m_maxHeightAchieved;

    public override void Enter() {
        base.Enter ();

        m_startingHeight = m_owner.transform.position.y;
        m_jumpCount++;
        m_jumpDuration = 0f;
    }

    public override void Exit() {
        base.Exit ();

        Debug.Log (string.Format ("Max height achieved: {0}", m_maxHeightAchieved));
        Debug.Log (string.Format ("Jump duration: {0}", m_jumpDuration));
    }

    public override void FixedUpdate() {
        base.FixedUpdate ();
        m_jumpDuration += Time.fixedDeltaTime;

        float newHeight = m_owner.transform.position.y - m_startingHeight;
        if (newHeight > m_maxHeightAchieved) {
            m_maxHeightAchieved = newHeight;
        }

        if (m_jumpCount == 0) {
            m_owner.ReplaceState (new PlatformerMotorStateLanded (m_owner, this));
            return;
        }

        if (HasRequestedMovementDirection ()) {
            // Debug.Log ("In motion: " + Velocity.ToString ());
            float maxXVelocity = RequestedMovementDirection.x * m_owner.jumpControlSpeed;
            float newXVelocity = m_owner.accelerationRate * maxXVelocity + (1f - m_owner.accelerationRate) * Velocity.x;
            Velocity = new Vector2(newXVelocity, Velocity.y);
        }

        if (IsHoldingJump () && m_jumpDuration < m_owner.maxJumpDuration) {
            Velocity = new Vector2(Velocity.x, m_owner.jumpForce + Velocity.y);
        }
        else if (IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateFalling (m_owner, this));
            return;
        }
        else if (m_owner.IsGrounded()) {
            m_owner.ReplaceState(new PlatformerMotorStateLanded(m_owner, this));
        }
    }
}
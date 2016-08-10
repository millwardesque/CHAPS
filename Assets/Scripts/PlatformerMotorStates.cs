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
    Vector2 velocityLastUpdate;

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

    public virtual void Enter() {
        // Debug.Log ("Entering " + GetType ().ToString ());
    }

    public virtual void HandleInput() {
        m_requestedMovementDirection = new Vector2 (m_owner.Controller.GetAxis ("Move Horizontal"), 0f);

        if (m_owner.Controller.GetButtonDown("Jump")) {
            m_hasRequestedJump = true;
            m_isHoldingJump = true;
        }
        else if (m_owner.Controller.GetButton ("Jump")) {
            m_isHoldingJump = true;
            m_hasRequestedJump = false;
        }
        else if (m_owner.Controller.GetButtonUp ("Jump")) {
            m_isHoldingJump = false;
            m_hasRequestedJump = false;
        }
    }

    public virtual void FixedUpdate() {
        if (m_owner.RB.velocity.x < -Mathf.Epsilon && velocityLastUpdate.x >= -Mathf.Epsilon) {
            m_owner.GetComponentInChildren<SpriteRenderer>().flipX = true;
        }
        else if (m_owner.RB.velocity.x > Mathf.Epsilon && velocityLastUpdate.x <= Mathf.Epsilon) {
            m_owner.GetComponentInChildren<SpriteRenderer>().flipX = false;
        }

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

        velocityLastUpdate = m_owner.RB.velocity;
    }

    public virtual void OnCollisionEnter2D(Collision2D col) {
        // Debug.Log ("Collided with " + col.collider.name);
    }

    public virtual void Exit() {
        // Debug.Log ("Exiting " + GetType ().ToString ());
    }

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

    public override void Enter() {
        base.Enter ();
        GameManager.Instance.Messenger.SendMessage (m_owner, "StartedRunning", (object)m_owner);
    }

    public override void Exit() {
        base.Enter ();
        GameManager.Instance.Messenger.SendMessage (m_owner, "StoppedRunning", (object)m_owner);
    }

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
            float maxXVelocity = RequestedMovementDirection.x * m_owner.maxSpeed;
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
    bool m_canJump = true;

    public PlatformerMotorStateFalling(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public PlatformerMotorStateFalling(PlatformerMotor owner, PlatformerMotorState previousState, bool canJump) : base(owner, previousState) {
        m_canJump = canJump;
    }

    public override void HandleInput() {
        base.HandleInput();

        if (m_canJump && CanJump()) {
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
    }

    public override void FixedUpdate() {
        base.FixedUpdate ();

        if (HasRequestedMovementDirection ()) {
            float maxXVelocity = RequestedMovementDirection.x * m_owner.airControlSpeed;
            float newXVelocity = m_owner.accelerationRate * maxXVelocity + (1f - m_owner.accelerationRate) * Velocity.x;
            Velocity = new Vector2(newXVelocity, Velocity.y);
        }

        if (m_owner.IsGrounded()) {
            m_owner.ReplaceState (new PlatformerMotorStateLanded(m_owner, this));
            return;
        }
    }

    public override void OnCollisionEnter2D(Collision2D col) {
        base.OnCollisionEnter2D (col);

        m_canJump = false;
        return;
    }
}

/// <summary>
/// State when the actor is jumping
/// </summary>
public class PlatformerMotorStateJumping : PlatformerMotorState {
    float m_maxJumpDuration = -1f;
    float m_startingHeight = 0f;
    float m_maxHeightAchieved;
    bool m_suppressJumpSound = false;

    public PlatformerMotorStateJumping(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public PlatformerMotorStateJumping(PlatformerMotor owner, PlatformerMotorState previousState, float maxJumpDuration, bool suppressJumpSound = false) : base(owner, previousState) {
        m_maxJumpDuration = maxJumpDuration;
        m_suppressJumpSound = suppressJumpSound;
    }

    public override void Enter() {
        base.Enter ();

        m_startingHeight = m_owner.transform.position.y;

        if (m_maxJumpDuration < 0f) {   // If we haven't overridden the max jump duration in the constructor, use the default.
            m_maxJumpDuration = m_owner.maxJumpDuration;    
        }

        m_jumpCount++;
        m_jumpDuration = 0f;
    
        if (!m_suppressJumpSound) {
            if (m_jumpCount > 1) {
                GameManager.Instance.Messenger.SendMessage (m_owner, "PlatformerMultiJumped", (object)m_owner);
            }
            else {
                GameManager.Instance.Messenger.SendMessage (m_owner, "PlatformerJumped", (object)m_owner);
            }    
        }
    }

    public override void HandleInput() {
        base.HandleInput();

        if (CanJump()) {
            m_owner.ReplaceState(new PlatformerMotorStateJumping(m_owner, this));
            return;
        }
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
            float maxXVelocity = RequestedMovementDirection.x * m_owner.airControlSpeed;
            float newXVelocity = m_owner.accelerationRate * maxXVelocity + (1f - m_owner.accelerationRate) * Velocity.x;
            Velocity = new Vector2(newXVelocity, Velocity.y);
        }

        if (m_jumpDuration < m_maxJumpDuration && (m_maxHeightAchieved < m_owner.minJumpHeight || (m_jumpCount == 1 && IsHoldingJump ()))) {
            float jumpForce = m_owner.jumpForce;
            Velocity = new Vector2(Velocity.x, jumpForce + Velocity.y);
        }
        else if (IsDescending ()) {
            m_owner.ReplaceState (new PlatformerMotorStateFalling (m_owner, this));
            return;
        }
        else if (m_owner.IsGrounded()) {
            m_owner.ReplaceState(new PlatformerMotorStateLanded(m_owner, this));
        }
    }

    public override void OnCollisionEnter2D(Collision2D col) {
        base.OnCollisionEnter2D (col);
        for (int i = 0; i < col.contacts.Length; ++i) {
            if (Mathf.Abs(col.contacts[i].normal.y - 1f) <= 0.1f) { // The normal can be off by a little bit depending on where the collision circle hits the point on the floor
                return;
            }
        }
        m_owner.ReplaceState (new PlatformerMotorStateFalling (m_owner, this, false));
        return;
    }
}


/// <summary>
/// State when the actor has landed
/// </summary>
public class PlatformerMotorStateLanded : PlatformerMotorState {
    public PlatformerMotorStateLanded(PlatformerMotor owner, PlatformerMotorState previousState) : base(owner, previousState) { }

    public override void Enter() {
        base.Enter();
        m_jumpCount = 0;
        m_jumpDuration = 0f;
        GameManager.Instance.Messenger.SendMessage (m_owner, "PlatformerLanded", (object)m_owner);
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
/// Stunned state when actor is stunned.
/// </summary>
public class PlatformerMotorStateStunned : PlatformerMotorState {
    float m_stunDuration;
    float m_elapsed = 0;

    public PlatformerMotorStateStunned(PlatformerMotor owner, PlatformerMotorState previousState, float stunDuration) : base(owner, previousState) {
        m_stunDuration = stunDuration;
    }

    public override void HandleInput() { 
        base.HandleInput ();

        m_elapsed += Time.deltaTime;
        if (m_elapsed > m_stunDuration) {
            m_owner.ReplaceState (new PlatformerMotorStateIdle (m_owner, this));
            return;
        }
    }
}


/// <summary>
/// Stunned state when actor is stunned.
/// </summary>
public class PlatformerMotorStateUseTerminal : PlatformerMotorState {
    float m_duration;
    float m_elapsed;

    public PlatformerMotorStateUseTerminal(PlatformerMotor owner, PlatformerMotorState previousState, float duration) : base(owner, previousState) {
        m_duration = duration;
    }

    public override void HandleInput() { 
        base.HandleInput ();

        m_elapsed += Time.deltaTime;
        if (m_elapsed > m_duration) {
            m_owner.ReplaceState (new PlatformerMotorStateIdle (m_owner, this));
            return;
        }
    }
}
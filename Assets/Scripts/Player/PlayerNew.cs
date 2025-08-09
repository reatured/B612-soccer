using UnityEngine;
using StateMachine;

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerPowerUps))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerBehaviorTreeRunner))]
public class PlayerNew : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    
    [Header("Architecture Settings")]
    public bool useStateMachine = true;
    public bool useBehaviorTree = false;
    public bool debugMode = true;
    
    // Component references (will be auto-assigned by PlayerStateMachine)
    private PlayerStateMachine stateMachine;
    private PlayerMovement movement;
    private PlayerPhysics physics;
    private PlayerAudio audio;
    private PlayerPowerUps powerUps;
    private PlayerInput input;
    private PlayerAnimator animator;
    private PlayerBehaviorTreeRunner behaviorTree;
    
    void Awake()
    {
        // Get all components
        stateMachine = GetComponent<PlayerStateMachine>();
        movement = GetComponent<PlayerMovement>();
        physics = GetComponent<PlayerPhysics>();
        audio = GetComponent<PlayerAudio>();
        powerUps = GetComponent<PlayerPowerUps>();
        input = GetComponent<PlayerInput>();
        animator = GetComponent<PlayerAnimator>();
        behaviorTree = GetComponent<PlayerBehaviorTreeRunner>();
    }
    
    void Start()
    {
        // Initialize the state machine with idle state
        if (useStateMachine)
        {
            stateMachine.ChangeState(new PlayerIdleState());
        }
        
        // Configure behavior tree
        if (behaviorTree != null)
        {
            behaviorTree.ToggleBehaviorTree(useBehaviorTree);
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} initialized with new architecture. StateMachine: {useStateMachine}, BehaviorTree: {useBehaviorTree}");
        }
    }
    
    void Update()
    {
        // The state machine and behavior tree handle their own updates
        // This Update method is kept minimal to just handle any global player logic
        
        if (!useStateMachine)
        {
            // Fallback: handle input manually if state machine is disabled
            HandleManualUpdate();
        }
    }
    
    void FixedUpdate()
    {
        // Let state machine handle physics, or do manual handling
        if (!useStateMachine)
        {
            HandleManualFixedUpdate();
        }
    }
    
    private void HandleManualUpdate()
    {
        // Fallback manual input handling (similar to original Player.cs)
        movement.CheckGrounded();
        
        if (input.IsMoving() && movement.isGrounded)
        {
            movement.MoveAroundPlanet(input.MoveInput);
            movement.FlipSprite(input.MoveInput);
            
            if (!audio.IsPlayingFootsteps())
            {
                audio.StartMovementAudio();
            }
        }
        else if (!input.IsMoving() && audio.IsPlayingFootsteps())
        {
            audio.StopMovementAudio();
        }
        
        if (input.JumpPressed && movement.isGrounded)
        {
            movement.Jump();
            audio.PlayJumpSound();
        }
        
        animator.UpdateAnimationState();
    }
    
    private void HandleManualFixedUpdate()
    {
        // Fallback manual physics handling
        physics.ApplyPlanetGravity();
        physics.OrientToPlanet(movement.isGrounded);
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Let the state machine handle collisions if enabled
        if (!useStateMachine)
        {
            // Manual collision handling
            Ball ball = collision.gameObject.GetComponent<Ball>();
            if (collision.contacts.Length > 0 && ball != null)
            {
                physics.HandleBallCollision(collision, ball);
                audio.PlayKickSound();
            }
        }
    }
    
    // Public methods for power-ups (maintaining compatibility with existing code)
    public void ApplyPowerUp(PowerUpType powerUpType)
    {
        if (useStateMachine && stateMachine.GetCurrentState() != null)
        {
            // Wrap current state in powered-up state
            var currentState = stateMachine.GetCurrentState();
            stateMachine.ChangeState(new PlayerPoweredUpState(currentState, powerUpType));
        }
        else
        {
            // Direct power-up application
            powerUps.ApplyPowerUp(powerUpType);
        }
    }
    
    // Utility methods to maintain compatibility
    public bool IsGrounded() => movement.isGrounded;
    public bool IsFacingRight() => movement.facingRight;
    public float GetMoveSpeed() => movement.moveSpeed;
    
    // Debug methods
    public string GetCurrentStateName()
    {
        return stateMachine != null ? stateMachine.currentStateName : "No State Machine";
    }
    
    public void ToggleStateMachine(bool enable)
    {
        useStateMachine = enable;
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} state machine toggled: {enable}");
        }
    }
    
    public void ToggleBehaviorTree(bool enable)
    {
        useBehaviorTree = enable;
        if (behaviorTree != null)
        {
            behaviorTree.ToggleBehaviorTree(enable);
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} behavior tree toggled: {enable}");
        }
    }
    
    public void EnableAI(bool enable)
    {
        if (behaviorTree != null)
        {
            behaviorTree.EnableAI(enable);
            useBehaviorTree = enable;
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} AI mode: {enable}");
        }
    }
}
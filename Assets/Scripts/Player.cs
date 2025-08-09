using UnityEngine;
using StateMachine;

public enum PlayerType
{
    Human,      // Human-controlled player
    AI,         // AI-controlled player
    Hybrid      // Can switch between human and AI
}

public enum AIDifficulty
{
    Beginner,   // Simple, predictable AI
    Normal,     // Balanced AI behavior
    Advanced,   // Challenging AI with good tactics
    Expert      // Near-perfect AI play
}

public enum AIPersonality
{
    Balanced,   // Well-rounded playstyle
    Aggressive, // Focuses on attacking and ball chasing
    Defensive,  // Prioritizes goal defense
    Opportunist // Waits for good chances, counter-attacks
}

[RequireComponent(typeof(PlayerStateMachine))]
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerPhysics))]
[RequireComponent(typeof(PlayerAudio))]
[RequireComponent(typeof(PlayerPowerUps))]
[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerAnimator))]
[RequireComponent(typeof(PlayerBehaviorTreeRunner))]
[RequireComponent(typeof(SoccerAI))]
public class Player : MonoBehaviour
{
    [Header("Player Configuration")]
    public int playerNumber = 1;
    public PlayerType playerType = PlayerType.Human;
    
    [Header("AI Settings")]
    public AIDifficulty aiDifficulty = AIDifficulty.Normal;
    public AIPersonality aiPersonality = AIPersonality.Balanced;
    [Range(0f, 1f)] public float aiReactionTime = 0.2f;
    [Range(0f, 1f)] public float aiSkillLevel = 0.7f;
    
    [Header("Runtime Controls")]
    public bool allowPlayerTypeSwitch = true;
    [Space]
    public KeyCode switchToHumanKey = KeyCode.H;
    public KeyCode switchToAIKey = KeyCode.B;
    
    [Header("Architecture Settings")]
    public bool useStateMachine = true;
    public bool useBehaviorTree = false;
    public bool debugMode = true;
    
    [Header("Legacy Compatibility - Will be moved to components")]
    public float moveSpeed = 3f;
    public float collisionKickForce = 15f;
    public float jumpForce = 8f;
    public float gravityStrength = 50f;
    public float gravityFadeDistance = 10f;
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 1.0f;
    public float groundCheckRadius = 0.3f;
    public AudioSource audioSource;
    public AudioClip kickBallSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip moveSound;
    public float powerKickMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float jumpMultiplier = 1f;
    public float powerUpDuration = 5f;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    
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
        
        // Copy legacy settings to new components
        CopyLegacySettings();
    }
    
    void Start()
    {
        // Configure player type
        ConfigurePlayerType();
        
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
            Debug.Log($"Player {playerNumber} initialized as {playerType} with new architecture. StateMachine: {useStateMachine}, BehaviorTree: {useBehaviorTree}");
        }
    }
    
    void Update()
    {
        // Handle runtime player type switching
        if (allowPlayerTypeSwitch)
        {
            HandlePlayerTypeSwitching();
        }
        
        // The state machine and behavior tree handle their own updates
        // This Update method is kept minimal to just handle any global player logic
        
        if (!useStateMachine)
        {
            // Fallback: handle input manually if state machine is disabled
            HandleManualUpdate();
        }
    }
    
    void ConfigurePlayerType()
    {
        switch (playerType)
        {
            case PlayerType.Human:
                SetupHumanPlayer();
                break;
            case PlayerType.AI:
                SetupAIPlayer();
                break;
            case PlayerType.Hybrid:
                SetupHybridPlayer();
                break;
        }
    }
    
    void SetupHumanPlayer()
    {
        input.SetInputSource(InputSource.Human);
        useStateMachine = true;
        useBehaviorTree = false;
        
        if (behaviorTree != null)
        {
            behaviorTree.ToggleBehaviorTree(false);
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} configured as Human player");
        }
    }
    
    void SetupAIPlayer()
    {
        input.SetInputSource(InputSource.AI);
        useStateMachine = true;
        useBehaviorTree = true;
        
        if (behaviorTree != null)
        {
            behaviorTree.ToggleBehaviorTree(true);
            behaviorTree.EnableAI(true);
            ConfigureAIBehavior();
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} configured as AI player - Difficulty: {aiDifficulty}, Personality: {aiPersonality}");
        }
    }
    
    void SetupHybridPlayer()
    {
        input.SetInputSource(InputSource.Hybrid);
        input.allowAIOverride = true;
        useStateMachine = true;
        useBehaviorTree = true;
        
        if (behaviorTree != null)
        {
            behaviorTree.ToggleBehaviorTree(true);
            behaviorTree.EnableAI(false); // Start with human control
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} configured as Hybrid player");
        }
    }
    
    void ConfigureAIBehavior()
    {
        if (behaviorTree == null) return;
        
        // Configure AI reaction time based on difficulty
        switch (aiDifficulty)
        {
            case AIDifficulty.Beginner:
                aiReactionTime = 0.5f;
                aiSkillLevel = 0.3f;
                break;
            case AIDifficulty.Normal:
                aiReactionTime = 0.3f;
                aiSkillLevel = 0.6f;
                break;
            case AIDifficulty.Advanced:
                aiReactionTime = 0.15f;
                aiSkillLevel = 0.8f;
                break;
            case AIDifficulty.Expert:
                aiReactionTime = 0.05f;
                aiSkillLevel = 0.95f;
                break;
        }
        
        // Apply AI settings to input component
        if (input != null)
        {
            input.aiInputSmoothTime = aiReactionTime;
        }
    }
    
    void HandlePlayerTypeSwitching()
    {
        // Quick switch keys for testing
        if (Input.GetKeyDown(switchToHumanKey))
        {
            SwitchToHuman();
        }
        else if (Input.GetKeyDown(switchToAIKey))
        {
            SwitchToAI();
        }
    }
    
    void CopyLegacySettings()
    {
        // Copy settings from legacy fields to new component system
        if (movement != null)
        {
            movement.moveSpeed = moveSpeed;
            movement.jumpForce = jumpForce;
            movement.jumpMultiplier = jumpMultiplier;
            movement.groundCheckDistance = groundCheckDistance;
            movement.groundCheckRadius = groundCheckRadius;
            movement.groundLayerMask = groundLayerMask;
        }
        
        if (physics != null)
        {
            physics.gravityStrength = gravityStrength;
            physics.gravityFadeDistance = gravityFadeDistance;
            physics.collisionKickForce = collisionKickForce;
        }
        
        if (audio != null)
        {
            audio.audioSource = audioSource;
            audio.kickBallSound = kickBallSound;
            audio.jumpSound = jumpSound;
            audio.landSound = landSound;
            audio.moveSound = moveSound;
        }
        
        if (powerUps != null)
        {
            powerUps.powerKickMultiplier = powerKickMultiplier;
            powerUps.sizeMultiplier = sizeMultiplier;
            powerUps.jumpMultiplier = jumpMultiplier;
            powerUps.powerUpDuration = powerUpDuration;
        }
        
        if (input != null)
        {
            input.leftKey = leftKey;
            input.rightKey = rightKey;
            input.jumpKey = jumpKey;
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
    
    // Player Type Management Methods
    public void SwitchToHuman()
    {
        if (!allowPlayerTypeSwitch) return;
        
        playerType = PlayerType.Human;
        SetupHumanPlayer();
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} switched to Human control");
        }
    }
    
    public void SwitchToAI()
    {
        if (!allowPlayerTypeSwitch) return;
        
        playerType = PlayerType.AI;
        SetupAIPlayer();
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} switched to AI control");
        }
    }
    
    public void SwitchToHybrid()
    {
        if (!allowPlayerTypeSwitch) return;
        
        playerType = PlayerType.Hybrid;
        SetupHybridPlayer();
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} switched to Hybrid control");
        }
    }
    
    public void SetPlayerType(PlayerType newType)
    {
        if (!allowPlayerTypeSwitch && newType != playerType) return;
        
        playerType = newType;
        ConfigurePlayerType();
    }
    
    public void SetAIDifficulty(AIDifficulty difficulty)
    {
        aiDifficulty = difficulty;
        if (playerType == PlayerType.AI || playerType == PlayerType.Hybrid)
        {
            ConfigureAIBehavior();
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} AI difficulty set to: {difficulty}");
        }
    }
    
    public void SetAIPersonality(AIPersonality personality)
    {
        aiPersonality = personality;
        if (playerType == PlayerType.AI || playerType == PlayerType.Hybrid)
        {
            ConfigureAIBehavior();
        }
        
        if (debugMode)
        {
            Debug.Log($"Player {playerNumber} AI personality set to: {personality}");
        }
    }
    
    // Information methods
    public bool IsHuman()
    {
        return playerType == PlayerType.Human || (playerType == PlayerType.Hybrid && input.IsHumanControlled());
    }
    
    public bool IsAI()
    {
        return playerType == PlayerType.AI || (playerType == PlayerType.Hybrid && input.IsAIControlled());
    }
    
    public string GetPlayerTypeDescription()
    {
        string description = $"{playerType}";
        if (playerType == PlayerType.AI || playerType == PlayerType.Hybrid)
        {
            description += $" ({aiDifficulty} {aiPersonality})";
        }
        return description;
    }
    
    public float GetAISkillLevel()
    {
        return aiSkillLevel;
    }
    
    public float GetAIReactionTime()
    {
        return aiReactionTime;
    }
}

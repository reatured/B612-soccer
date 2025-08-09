using UnityEngine;
using BehaviorTree;
using BehaviorTree.Player;
using StateMachine;

public class PlayerBehaviorTreeRunner : BehaviorTreeRunner
{
    [Header("Player Behavior Tree")]
    public bool enableBehaviorTree = true;
    public bool useAutonomousAI = false; // When true, uses BT for AI; when false, uses for input processing
    
    [Header("AI Integration")]
    public float decisionUpdateRate = 0.1f; // How often AI makes decisions (in seconds)
    public bool integrateWithStateMachine = true;
    
    private PlayerStateMachine playerStateMachine;
    private Player player;
    private SoccerAI soccerAI;
    private float lastDecisionTime;
    private float aiDecisionCooldown = 0f;
    
    void Awake()
    {
        playerStateMachine = GetComponent<PlayerStateMachine>();
        player = GetComponent<Player>();
        soccerAI = GetComponent<SoccerAI>();
        
        // Add SoccerAI component if it doesn't exist
        if (soccerAI == null)
        {
            soccerAI = gameObject.AddComponent<SoccerAI>();
        }
    }
    
    void Update()
    {
        base.Update(); // Run the behavior tree
        
        // Handle AI decision cooldown based on difficulty
        if (useAutonomousAI && player != null)
        {
            aiDecisionCooldown = player.GetAIReactionTime();
            
            // Only make decisions at the specified rate
            if (Time.time - lastDecisionTime >= Mathf.Max(decisionUpdateRate, aiDecisionCooldown))
            {
                lastDecisionTime = Time.time;
                ProcessAIDecisions();
            }
        }
    }
    
    void ProcessAIDecisions()
    {
        if (!integrateWithStateMachine || playerStateMachine == null || playerStateMachine.input == null)
            return;
        
        // Get AI decisions from behavior tree and inject them as input
        float moveDecision = CalculateAIMoveDecision();
        bool jumpDecision = CalculateAIJumpDecision();
        
        // Inject AI decisions into the input system
        playerStateMachine.input.SetAIMove(moveDecision);
        if (jumpDecision)
        {
            playerStateMachine.input.TriggerAIJump();
        }
    }
    
    float CalculateAIMoveDecision()
    {
        if (soccerAI != null)
        {
            // Use advanced soccer AI logic
            return soccerAI.GetOptimalMoveDirection();
        }
        
        // Fallback to simple ball chasing
        Ball ball = FindBall();
        if (ball == null) return 0f;
        
        Vector2 ballPosition = ball.transform.position;
        Vector2 playerPosition = transform.position;
        Vector2 directionToBall = ballPosition - playerPosition;
        
        float ballDistance = directionToBall.magnitude;
        if (ballDistance < GetChaseDistance())
        {
            return Mathf.Sign(directionToBall.x);
        }
        
        return 0f;
    }
    
    bool CalculateAIJumpDecision()
    {
        if (soccerAI != null)
        {
            // Use advanced soccer AI logic
            return soccerAI.ShouldJumpForBall();
        }
        
        // Fallback to simple jump logic
        Ball ball = FindBall();
        if (ball == null || !playerStateMachine.movement.isGrounded) return false;
        
        Vector2 ballPosition = ball.transform.position;
        Vector2 playerPosition = transform.position;
        float ballDistance = Vector2.Distance(ballPosition, playerPosition);
        
        return ballPosition.y > playerPosition.y + 1f && ballDistance < 3f;
    }
    
    float GetChaseDistance()
    {
        AIPersonality personality = player.aiPersonality;
        float baseDistance = 8f;
        
        switch (personality)
        {
            case AIPersonality.Aggressive:
                return baseDistance * 1.5f; // Chase farther
            case AIPersonality.Defensive:
                return baseDistance * 0.7f; // Stay closer to goal
            case AIPersonality.Opportunist:
                return baseDistance * 0.9f; // Moderate chasing
            default:
                return baseDistance;
        }
    }
    
    float ApplyPersonalityToMovement(float moveDirection, float ballDistance)
    {
        AIPersonality personality = player.aiPersonality;
        
        switch (personality)
        {
            case AIPersonality.Aggressive:
                // Always move towards ball more aggressively
                return moveDirection * 1.2f;
                
            case AIPersonality.Defensive:
                // Only move if ball is very close or if far from own goal
                Vector2 goalPosition = GetOwnGoalPosition();
                float distanceFromGoal = Vector2.Distance(transform.position, goalPosition);
                if (distanceFromGoal > 6f && ballDistance > 4f)
                {
                    // Move back towards goal
                    return Mathf.Sign(goalPosition.x - transform.position.x) * 0.6f;
                }
                return ballDistance < 4f ? moveDirection * 0.8f : 0f;
                
            case AIPersonality.Opportunist:
                // Only move when ball is close or when it's a good opportunity
                return ballDistance < 5f ? moveDirection : 0f;
                
            default:
                return moveDirection;
        }
    }
    
    Vector2 GetOwnGoalPosition()
    {
        // This is a simplified goal position calculation
        // In a real game, you'd get the actual goal position based on player team
        return player.playerNumber == 1 ? new Vector2(-8f, 0f) : new Vector2(8f, 0f);
    }
    
    Ball FindBall()
    {
        return Object.FindFirstObjectByType<Ball>();
    }
    
    public override BTNode SetupTree()
    {
        if (!enableBehaviorTree || playerStateMachine == null)
            return null;
        
        if (useAutonomousAI)
        {
            return SetupAIBehaviorTree();
        }
        else
        {
            return SetupInputProcessingTree();
        }
    }
    
    private BTNode SetupInputProcessingTree()
    {
        // This tree processes input and decides actions based on player input
        return new BTSelector("InputProcessing")
            .AddChild(
                // Jump if jump is pressed and grounded
                new BTSequence("JumpSequence")
                    .AddChild(new CanJumpCondition(playerStateMachine))
                    .AddChild(new JumpAction(playerStateMachine))
            )
            .AddChild(
                // Move if moving input is detected and grounded
                new BTSequence("MoveSequence")
                    .AddChild(new IsGroundedCondition(playerStateMachine))
                    .AddChild(new IsMovingCondition(playerStateMachine))
                    .AddChild(new MoveAction(playerStateMachine, playerStateMachine.input.MoveInput))
            )
            .AddChild(
                // Default idle behavior
                new IdleAction(playerStateMachine)
            );
    }
    
    private BTNode SetupAIBehaviorTree()
    {
        // This tree creates autonomous AI behavior for the player
        return new BTSelector("AI_Root")
            .AddChild(
                // Priority 1: If ball is very close and we can kick it
                new BTSequence("KickBall")
                    .AddChild(new BallNearbyCondition(playerStateMachine, 2f))
                    .AddChild(new IsGroundedCondition(playerStateMachine))
                    .AddChild(new KickBallAction(playerStateMachine, Object.FindFirstObjectByType<Ball>()))
            )
            .AddChild(
                // Priority 2: Move towards ball if it's nearby
                new BTSequence("ChaseBall")
                    .AddChild(new BallNearbyCondition(playerStateMachine, 10f))
                    .AddChild(new IsGroundedCondition(playerStateMachine))
                    .AddChild(new MoveAction(playerStateMachine, GetDirectionToBall()))
            )
            .AddChild(
                // Priority 3: Jump if we're stuck or for variety
                new BTSequence("RandomJump")
                    .AddChild(new IsGroundedCondition(playerStateMachine))
                    .AddChild(new BTCooldown("JumpCooldown", 3f)
                        .SetChild(new JumpAction(playerStateMachine)))
            )
            .AddChild(
                // Default: Idle or random movement
                new BTSelector("DefaultBehavior")
                    .AddChild(new IdleAction(playerStateMachine))
            );
    }
    
    private float GetDirectionToBall()
    {
        Ball ball = Object.FindFirstObjectByType<Ball>();
        if (ball == null || playerStateMachine == null)
            return 0f;
        
        Vector2 directionToBall = ball.transform.position - playerStateMachine.transform.position;
        return Mathf.Sign(directionToBall.x);
    }
    
    public void EnableAI(bool enable)
    {
        useAutonomousAI = enable;
        
        // Rebuild the tree with new settings
        if (enableBehaviorTree)
        {
            SetRootNode(SetupTree());
        }
    }
    
    public void ToggleBehaviorTree(bool enable)
    {
        enableBehaviorTree = enable;
        
        if (enable)
        {
            SetRootNode(SetupTree());
        }
        else
        {
            SetRootNode(null);
        }
    }
}
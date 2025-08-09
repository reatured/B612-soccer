using UnityEngine;
using StateMachine;

public class SoccerAI : MonoBehaviour
{
    [Header("Soccer AI Settings")]
    public float ballPredictionTime = 0.5f;
    public float positioningRadius = 5f;
    public float goalDefenseRadius = 4f;
    
    [Header("Tactical Settings")]
    public float offensiveThreshold = 0.6f; // When to be aggressive
    public float defensiveThreshold = 0.3f; // When to be defensive
    public float passConsiderationDistance = 8f;
    
    private Player player;
    private Ball ball;
    private PlayerStateMachine stateMachine;
    
    // Cached calculations
    private Vector2 predictedBallPosition;
    private Vector2 optimalPosition;
    private float tacticalState; // 0 = defensive, 1 = offensive
    
    void Awake()
    {
        player = GetComponent<Player>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }
    
    void Start()
    {
        ball = FindFirstObjectByType<Ball>();
    }
    
    void Update()
    {
        if (player.IsAI() && ball != null)
        {
            UpdateTacticalAnalysis();
        }
    }
    
    void UpdateTacticalAnalysis()
    {
        // Update ball prediction
        predictedBallPosition = PredictBallPosition();
        
        // Calculate optimal positioning
        optimalPosition = CalculateOptimalPosition();
        
        // Update tactical state based on game situation
        tacticalState = CalculateTacticalState();
    }
    
    public Vector2 PredictBallPosition()
    {
        if (ball == null) return Vector2.zero;
        
        Vector2 ballPos = ball.transform.position;
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        
        if (ballRb != null)
        {
            // Predict where ball will be based on current velocity
            Vector2 ballVelocity = ballRb.linearVelocity;
            return ballPos + ballVelocity * ballPredictionTime;
        }
        
        return ballPos;
    }
    
    public Vector2 CalculateOptimalPosition()
    {
        Vector2 currentPos = transform.position;
        Vector2 ballPos = ball.transform.position;
        Vector2 goalPos = GetOwnGoalPosition();
        Vector2 enemyGoalPos = GetEnemyGoalPosition();
        
        Vector2 optimalPos = currentPos;
        
        switch (player.aiPersonality)
        {
            case AIPersonality.Aggressive:
                optimalPos = CalculateAggressivePosition(ballPos, enemyGoalPos);
                break;
                
            case AIPersonality.Defensive:
                optimalPos = CalculateDefensivePosition(ballPos, goalPos);
                break;
                
            case AIPersonality.Balanced:
                optimalPos = CalculateBalancedPosition(ballPos, goalPos, enemyGoalPos);
                break;
                
            case AIPersonality.Opportunist:
                optimalPos = CalculateOpportunisticPosition(ballPos, goalPos, enemyGoalPos);
                break;
        }
        
        return optimalPos;
    }
    
    Vector2 CalculateAggressivePosition(Vector2 ballPos, Vector2 enemyGoal)
    {
        // Always try to be between ball and enemy goal, but closer to ball
        Vector2 ballToGoal = (enemyGoal - ballPos).normalized;
        Vector2 targetPos = ballPos + ballToGoal * 2f;
        
        // Add some randomness based on skill level
        if (player.GetAISkillLevel() < 0.8f)
        {
            float randomOffset = (1f - player.GetAISkillLevel()) * 2f;
            targetPos += new Vector2(
                Random.Range(-randomOffset, randomOffset),
                Random.Range(-randomOffset, randomOffset)
            );
        }
        
        return targetPos;
    }
    
    Vector2 CalculateDefensivePosition(Vector2 ballPos, Vector2 ownGoal)
    {
        // Stay between ball and own goal
        Vector2 ballToGoal = (ownGoal - ballPos).normalized;
        float defenseDistance = Mathf.Min(goalDefenseRadius, Vector2.Distance(ballPos, ownGoal) * 0.5f);
        
        return ownGoal - ballToGoal * defenseDistance;
    }
    
    Vector2 CalculateBalancedPosition(Vector2 ballPos, Vector2 ownGoal, Vector2 enemyGoal)
    {
        // Mix of aggressive and defensive based on ball position
        float fieldPosition = Mathf.InverseLerp(ownGoal.x, enemyGoal.x, ballPos.x);
        
        if (player.playerNumber == 2) // Right side player
            fieldPosition = 1f - fieldPosition;
        
        if (fieldPosition > 0.6f) // Ball is in enemy territory
        {
            return CalculateAggressivePosition(ballPos, enemyGoal);
        }
        else if (fieldPosition < 0.4f) // Ball is in own territory
        {
            return CalculateDefensivePosition(ballPos, ownGoal);
        }
        else // Ball in middle
        {
            Vector2 midFieldPos = Vector2.Lerp(ownGoal, enemyGoal, 0.5f);
            Vector2 toBall = (ballPos - midFieldPos).normalized;
            return midFieldPos + toBall * 3f;
        }
    }
    
    Vector2 CalculateOpportunisticPosition(Vector2 ballPos, Vector2 ownGoal, Vector2 enemyGoal)
    {
        // Wait for good opportunities, don't chase aggressively
        Vector2 currentPos = transform.position;
        float distanceToBall = Vector2.Distance(currentPos, ballPos);
        
        if (distanceToBall < 4f) // Close enough to act
        {
            return CalculateAggressivePosition(ballPos, enemyGoal);
        }
        else
        {
            // Stay in a good position to intercept
            Vector2 midPoint = Vector2.Lerp(ownGoal, ballPos, 0.7f);
            return midPoint;
        }
    }
    
    float CalculateTacticalState()
    {
        Vector2 ballPos = ball.transform.position;
        Vector2 ownGoal = GetOwnGoalPosition();
        Vector2 enemyGoal = GetEnemyGoalPosition();
        
        // Calculate based on ball position relative to field
        float fieldPosition = Mathf.InverseLerp(ownGoal.x, enemyGoal.x, ballPos.x);
        
        if (player.playerNumber == 2) // Right side player
            fieldPosition = 1f - fieldPosition;
        
        // Additional factors
        float ballDistance = Vector2.Distance(transform.position, ballPos);
        float ballSpeed = ball.GetComponent<Rigidbody2D>()?.linearVelocity.magnitude ?? 0f;
        
        // Combine factors
        float tactical = fieldPosition;
        tactical += (ballDistance < 5f ? 0.2f : -0.1f); // Closer = more offensive
        tactical += (ballSpeed > 5f ? -0.1f : 0.1f); // Fast ball = more defensive
        
        return Mathf.Clamp01(tactical);
    }
    
    public bool ShouldChaseAggressively()
    {
        return tacticalState > offensiveThreshold;
    }
    
    public bool ShouldDefend()
    {
        return tacticalState < defensiveThreshold;
    }
    
    public bool ShouldPositionForIntercept()
    {
        Vector2 ballVelocity = ball.GetComponent<Rigidbody2D>()?.linearVelocity ?? Vector2.zero;
        
        // Check if ball is moving and we can intercept it
        if (ballVelocity.magnitude > 2f)
        {
            Vector2 interceptPoint = PredictBallPosition();
            float timeToIntercept = Vector2.Distance(transform.position, interceptPoint) / stateMachine.movement.moveSpeed;
            float ballTimeToPoint = ballPredictionTime;
            
            // We can intercept if we'll get there around the same time
            return Mathf.Abs(timeToIntercept - ballTimeToPoint) < 0.5f;
        }
        
        return false;
    }
    
    public float GetOptimalMoveDirection()
    {
        Vector2 currentPos = transform.position;
        Vector2 targetPos = optimalPosition;
        
        // Apply some AI imperfection based on skill level
        float skillLevel = player.GetAISkillLevel();
        if (skillLevel < 1f)
        {
            float errorMargin = (1f - skillLevel) * 2f;
            targetPos += new Vector2(
                Random.Range(-errorMargin, errorMargin),
                Random.Range(-errorMargin, errorMargin)
            );
        }
        
        float direction = Mathf.Sign(targetPos.x - currentPos.x);
        float distance = Mathf.Abs(targetPos.x - currentPos.x);
        
        // Don't move if we're close enough to target
        if (distance < 0.5f) return 0f;
        
        // Reduce movement speed if we're close to target
        if (distance < 2f)
        {
            direction *= (distance / 2f);
        }
        
        return Mathf.Clamp(direction, -1f, 1f);
    }
    
    public bool ShouldJumpForBall()
    {
        Vector2 ballPos = predictedBallPosition;
        Vector2 currentPos = transform.position;
        
        // Jump if ball is above us and within reach
        bool ballAbove = ballPos.y > currentPos.y + 1f;
        bool ballClose = Vector2.Distance(ballPos, currentPos) < 3f;
        bool isGrounded = stateMachine.movement.isGrounded;
        
        if (ballAbove && ballClose && isGrounded)
        {
            // Apply skill level check
            float skillLevel = player.GetAISkillLevel();
            float jumpChance = skillLevel * 0.8f + 0.2f; // 20% to 100% chance
            return Random.value < jumpChance;
        }
        
        return false;
    }
    
    Vector2 GetOwnGoalPosition()
    {
        // Simplified goal positions - adjust based on your field layout
        return player.playerNumber == 1 ? new Vector2(-10f, 0f) : new Vector2(10f, 0f);
    }
    
    Vector2 GetEnemyGoalPosition()
    {
        // Simplified goal positions - adjust based on your field layout
        return player.playerNumber == 1 ? new Vector2(10f, 0f) : new Vector2(-10f, 0f);
    }
    
    // Public getters for external components
    public Vector2 GetPredictedBallPosition() => predictedBallPosition;
    public Vector2 GetOptimalPosition() => optimalPosition;
    public float GetTacticalState() => tacticalState;
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !player.IsAI()) return;
        
        // Draw predicted ball position
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(predictedBallPosition, 0.3f);
        
        // Draw optimal position
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(optimalPosition, 0.5f);
        Gizmos.DrawLine(transform.position, optimalPosition);
        
        // Draw chase distance based on personality
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, positioningRadius);
    }
}
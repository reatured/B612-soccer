using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Goal Settings")]
    public BallSpawner ballSpawner; // Reference to ball spawner
    public int goalOwner = 1; // 1 for Player 1's goal, 2 for Player 2's goal
    public float resetDelay = 2f;
    
    [Header("Effects")]
    public ParticleSystem goalEffect;
    public AudioSource goalSound;
    
    [Header("Goal Validation")]
    public string validGoalColliderTag = "GoalArea";
    public string preventionColliderTag = "TopArea";
    public bool showBlockedGoalFeedback = true;
    
    private bool goalScored = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (goalScored) return;
        
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            HandleBallCollision(ball, other);
        }
    }
    
    void HandleBallCollision(Ball ball, Collider2D hitCollider)
    {
        if (goalScored) return;
        
        // Check which collider was hit using tags
        if (hitCollider.CompareTag(validGoalColliderTag))
        {
            // Valid goal area - allow scoring
            Debug.Log($"Ball hit valid goal area: {hitCollider.name}");
            ScoreGoal(ball);
        }
        else if (hitCollider.CompareTag(preventionColliderTag))
        {
            // Prevention area (top/crossbar) - block goal
            Debug.Log($"Ball hit prevention area: {hitCollider.name} - Goal blocked!");
            HandleBlockedGoal(ball);
        }
        else
        {
            // Fallback: if no tags are set, use collider names
            string colliderName = hitCollider.name.ToLower();
            
            if (colliderName.Contains("goal") && !colliderName.Contains("top"))
            {
                Debug.Log($"Ball hit goal area by name: {hitCollider.name}");
                ScoreGoal(ball);
            }
            else if (colliderName.Contains("top") || colliderName.Contains("prevent"))
            {
                Debug.Log($"Ball hit prevention area by name: {hitCollider.name} - Goal blocked!");
                HandleBlockedGoal(ball);
            }
            else
            {
                // Default behavior for backward compatibility
                Debug.Log($"Ball hit untagged collider: {hitCollider.name} - Allowing goal");
                ScoreGoal(ball);
            }
        }
    }
    
    void ScoreGoal(Ball ball)
    {
        if (goalScored) return;
        
        goalScored = true;
        
        int scoringPlayer = (goalOwner == 1) ? 2 : 1;
        
        Debug.Log($"GOAL! Player {scoringPlayer} scored against Player {goalOwner}'s goal!");
        
        GameManager gameManager = FindFirstObjectByType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerScored(scoringPlayer);
        }
        
        PlayGoalEffects();
        
        // Stop ball physics and destroy after delay
        StopBallPhysics(ball);
        Destroy(ball.gameObject, 0.5f);        
        Invoke(nameof(ResetGoal), resetDelay);
    }
    
    void HandleBlockedGoal(Ball ball)
    {
        Debug.Log($"Goal attempt blocked! Ball hit the prevention area.");
        
        if (showBlockedGoalFeedback)
        {
            PlayBlockedGoalEffects();
        }
        
        // Ball continues in play - no destruction or goal scoring
        // Optionally modify ball physics (bounce back, etc.)
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            // Add a slight downward force to simulate hitting the crossbar
            Vector2 bounceForce = new(ballRb.linearVelocity.x * 0.5f, -Mathf.Abs(ballRb.linearVelocity.y) * 0.8f);
            ballRb.linearVelocity = bounceForce;
        }
    }
    
    void StopBallPhysics(Ball ball)
    {
        if (ball == null) return;
        
        // Get the ball's rigidbody and stop all physics
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb != null)
        {
            // Stop all movement and rotation
            ballRb.linearVelocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
            
            // Optionally freeze the rigidbody to prevent any further physics interactions
            ballRb.constraints = RigidbodyConstraints2D.FreezeAll;
            
            Debug.Log($"Ball physics stopped for goal scoring ball");
        }
        
        // Disable the ball's collider to prevent further interactions
        Collider2D ballCollider = ball.GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            ballCollider.enabled = false;
        }
        
        // Disable any trail effects on the ball
        TrailRenderer ballTrail = ball.GetComponent<TrailRenderer>();
        if (ballTrail != null)
        {
            ballTrail.enabled = false;
        }
    }
    
    void PlayBlockedGoalEffects()
    {
        // Visual feedback for blocked goal
        if (VisualEffectsManager.Instance != null)
        {
            // Play a different effect for blocked goals
            VisualEffectsManager.Instance.PlayGoalEffect(transform.position, 0); // 0 for blocked
        }
        
        // Audio feedback for blocked goal
        if (AudioManager.Instance != null)
        {
            // Play blocked goal sound (you'll need to add this to AudioManager)
            // AudioManager.Instance.PlayBlockedGoalSound();
        }
        
        Debug.Log("Played blocked goal effects");
    }
    
    void PlayGoalEffects()
    {
        if (goalEffect != null)
        {
            goalEffect.Play();
        }
        
        // Use AudioManager for goal sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayGoalSound();
        }
        else if (goalSound != null)
        {
            goalSound.Play();
        }
        
        // Play visual effects
        if (VisualEffectsManager.Instance != null)
        {
            int scoringPlayer = (goalOwner == 1) ? 2 : 1;
            VisualEffectsManager.Instance.PlayGoalEffect(transform.position, scoringPlayer);
        }
    }
    
    void ResetGoal()
    {
        goalScored = false;
        
        // Spawn a new ball using the ball spawner
        if (ballSpawner != null)
        {
            ballSpawner.SpawnBall();
        }
        else
        {
            Debug.LogWarning("No ball spawner assigned to Goal!");
        }
    }
    
    void OnDrawGizmos()
    {
        // Draw all colliders in this goal
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
        
        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag(validGoalColliderTag))
            {
                // Valid goal area - green
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                Gizmos.color = new Color(0, 1, 0, 0.2f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
            else if (col.CompareTag(preventionColliderTag))
            {
                // Prevention area - red
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
                Gizmos.color = new Color(1, 0, 0, 0.2f);
                Gizmos.DrawCube(col.bounds.center, col.bounds.size);
            }
            else
            {
                // Default - team color
                Gizmos.color = goalOwner == 1 ? Color.blue : Color.cyan;
                Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
            }
        }
        
        // Fallback if no child colliders found
        if (colliders.Length == 0)
        {
            Gizmos.color = goalOwner == 1 ? Color.blue : Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
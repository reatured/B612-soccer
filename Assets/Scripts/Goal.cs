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
    
    private bool goalScored = false;
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (goalScored) return;
        
        Ball ball = other.GetComponent<Ball>();
        if (ball != null)
        {
            ScoreGoal(ball);
        }
    }
    
    void ScoreGoal(Ball ball)
    {
        if (goalScored) return;
        
        goalScored = true;
        
        int scoringPlayer = (goalOwner == 1) ? 2 : 1;
        
        Debug.Log($"GOAL! Player {scoringPlayer} scored against Player {goalOwner}'s goal!");
        
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.PlayerScored(scoringPlayer);
        }
        
        PlayGoalEffects();
        
        // Make ball disappear immediately
        Destroy(ball.gameObject);        
        Invoke("ResetGoal", resetDelay);
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
        Gizmos.color = goalOwner == 1 ? Color.blue : Color.red;
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawWireCube(transform.position, col.bounds.size);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position, Vector3.one);
        }
    }
}
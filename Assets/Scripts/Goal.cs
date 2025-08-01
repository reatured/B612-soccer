using UnityEngine;

public class Goal : MonoBehaviour
{
    [Header("Goal Settings")]
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
            ScoreGoal();
        }
    }
    
    void ScoreGoal()
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
        
        Invoke("ResetGoal", resetDelay);
    }
    
    void PlayGoalEffects()
    {
        if (goalEffect != null)
        {
            goalEffect.Play();
        }
        
        if (goalSound != null)
        {
            goalSound.Play();
        }
    }
    
    void ResetGoal()
    {
        goalScored = false;
        
        Ball ball = FindObjectOfType<Ball>();
        if (ball != null)
        {
            ball.ResetBall();
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
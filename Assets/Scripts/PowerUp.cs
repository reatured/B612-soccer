using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    public PowerUpType powerUpType;
    public bool requiresBallHit = false; // true = ball must hit it, false = player can collect it
    public float rotationSpeed = 45f;
    public ParticleSystem collectEffect;
    public AudioClip collectSound;
    
    [Header("Visual")]
    public Color powerUpColor = Color.yellow;
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.2f;
    
    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool collected = false;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        // Set color based on power-up type
        if (spriteRenderer != null)
        {
            switch (powerUpType)
            {
                case PowerUpType.PowerfulKick:
                    spriteRenderer.color = Color.red;
                    break;
                case PowerUpType.BiggerBody:
                    spriteRenderer.color = Color.blue;
                    break;
                case PowerUpType.HigherJump:
                    spriteRenderer.color = Color.green;
                    break;
            }
        }
    }
    
    void Update()
    {
        // Rotate the power-up
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
        
        // Pulse effect
        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        transform.localScale = originalScale * pulse;
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        
        if (!requiresBallHit)
        {
            // Player collection
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                CollectPowerUp(player);
            }
        }
        else
        {
            // Ball collection
            Ball ball = other.GetComponent<Ball>();
            if (ball != null)
            {
                // Find closest player to give power-up to
                Player closestPlayer = FindClosestPlayer();
                if (closestPlayer != null)
                {
                    CollectPowerUp(closestPlayer);
                }
            }
        }
    }
    
    void CollectPowerUp(Player player)
    {
        if (collected) return;
        collected = true;
        
        player.ApplyPowerUp(powerUpType);
        
        // Play power-up visual and audio effects
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayPowerUpCollectEffect(transform.position);
            VisualEffectsManager.Instance.PlayPowerUpActivateEffect(player.transform);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayPowerUpSound();
        }        
        if (collectEffect != null)
        {
            collectEffect.Play();
        }
        
        if (collectSound != null && player.audioSource != null)
        {
            player.audioSource.PlayOneShot(collectSound);
        }
        
        Debug.Log($"Player {player.playerNumber} collected {powerUpType} power-up!");
        
        // Hide the power-up (or destroy after particle effect)
        GetComponent<Collider2D>().enabled = false;
        if (spriteRenderer != null)
            spriteRenderer.enabled = false;
            
        Destroy(gameObject, 2f); // Destroy after particle effect
    }
    
    Player FindClosestPlayer()
    {
        Player[] players = FindObjectsOfType<Player>();
        Player closest = null;
        float closestDistance = Mathf.Infinity;
        
        foreach (Player player in players)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = player;
            }
        }
        
        return closest;
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = requiresBallHit ? Color.cyan : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        
        // Draw icon based on type
        switch (powerUpType)
        {
            case PowerUpType.PowerfulKick:
                Gizmos.color = Color.red;
                break;
            case PowerUpType.BiggerBody:
                Gizmos.color = Color.blue;
                break;
            case PowerUpType.HigherJump:
                Gizmos.color = Color.green;
                break;
        }
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}
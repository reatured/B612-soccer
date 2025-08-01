using UnityEngine;

public class BallSpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject ballPrefab;
    public float spawnRadius = 2f;
    public Vector3 spawnOffset = Vector3.up;
    
    [Header("Auto Spawn")]
    public bool autoSpawnOnStart = true;
    public bool limitMaxBalls = true;
    public int maxBallsInScene = 1;
    
    [Header("Spawn Position")]
    public bool useRandomPosition = false;
    public bool spawnRelativeToPlanet = true;
    
    void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnBall();
        }
    }
    
    public GameObject SpawnBall()
    {
        if (ballPrefab == null)
        {
            Debug.LogWarning("No ball prefab assigned to BallSpawner!");
            return null;
        }
        
        if (limitMaxBalls && GetBallCount() >= maxBallsInScene)
        {
            Debug.Log($"Maximum balls ({maxBallsInScene}) already in scene. Not spawning new ball.");
            return null;
        }
        
        Vector3 spawnPosition = GetSpawnPosition();
        GameObject newBall = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        
        // Initialize the ball
        Ball ballComponent = newBall.GetComponent<Ball>();
        if (ballComponent != null)
        {
            ballComponent.ResetBall();
        }
        
        Debug.Log($"Ball spawned at position: {spawnPosition}");
        return newBall;
    }
    
    Vector3 GetSpawnPosition()
    {
        Vector3 basePosition = transform.position;
        
        if (spawnRelativeToPlanet)
        {
            Planet planet = FindObjectOfType<Planet>();
            if (planet != null)
            {
                // Spawn at a safe distance from planet surface
                Vector3 directionFromPlanet = (basePosition - planet.center.position).normalized;
                basePosition = planet.center.position + directionFromPlanet * (planet.radius + spawnRadius);
            }
        }
        
        Vector3 finalPosition = basePosition + spawnOffset;
        
        if (useRandomPosition)
        {
            // Add random offset within spawn radius
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            finalPosition += new Vector3(randomCircle.x, randomCircle.y, 0);
        }
        
        return finalPosition;
    }
    
    int GetBallCount()
    {
        Ball[] ballsInScene = FindObjectsOfType<Ball>();
        return ballsInScene.Length;
    }
    
    public void SpawnBallDelayed(float delay)
    {
        Invoke(nameof(SpawnBall), delay);
    }
    
    public void DestroyAllBalls()
    {
        Ball[] ballsInScene = FindObjectsOfType<Ball>();
        foreach (Ball ball in ballsInScene)
        {
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }
        }
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
        
        // Draw spawn position preview
        Vector3 previewSpawnPos = GetSpawnPosition();
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(previewSpawnPos, 0.2f);
        
        // Draw line to show spawn offset
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, previewSpawnPos);
    }
    
    void OnDrawGizmosSelected()
    {
        // Draw detailed spawn area when selected
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawSphere(transform.position, spawnRadius);
    }
}

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
    public int maxBallsInScene = 6;
    
    [Header("Random Spawning")]
    public bool enableRandomSpawning = true;
    public float minSpawnInterval = 5f;
    public float maxSpawnInterval = 15f;
    public bool onlySpawnDuringGameplay = true;
    
    [Header("Maintenance Spawning")]
    public bool enableMaintenanceSpawning = true;
    public int targetBallCount = 4;
    public int lowBallThreshold = 2;
    public float maintenanceCheckInterval = 2f;
    public float urgentSpawnDelay = 1f;
    
    [Header("Spawn Position")]
    public bool useRandomPosition = false;
    public bool spawnRelativeToPlanet = true;
    
    private float nextRandomSpawnTime;
    private float nextMaintenanceCheckTime;
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = FindFirstObjectByType<GameManager>();
        
        if (autoSpawnOnStart)
        {
            SpawnBall();
        }
        
        if (enableRandomSpawning)
        {
            ScheduleNextRandomSpawn();
        }
        
        if (enableMaintenanceSpawning)
        {
            ScheduleNextMaintenanceCheck();
        }
    }
    
    void Update()
    {
        // Random spawning system
        if (enableRandomSpawning && Time.time >= nextRandomSpawnTime)
        {
            if (ShouldSpawnRandomBall())
            {
                SpawnRandomBall();
            }
            ScheduleNextRandomSpawn();
        }
        
        // Maintenance spawning system
        if (enableMaintenanceSpawning && Time.time >= nextMaintenanceCheckTime)
        {
            CheckAndMaintainBallCount();
            ScheduleNextMaintenanceCheck();
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
            Planet planet = FindFirstObjectByType<Planet>();
            if (planet != null)
            {
                // Generate random angle around the planet
                float randomAngle = Random.Range(0f, 2f * Mathf.PI);
                
                // Calculate spawn distance (planet radius + spawn radius for safety)
                float spawnDistance = planet.radius + spawnRadius;
                
                // Calculate position around the planet
                Vector3 planetCenter = planet.center.position;
                basePosition = new Vector3(
                    planetCenter.x + Mathf.Cos(randomAngle) * spawnDistance,
                    planetCenter.y + Mathf.Sin(randomAngle) * spawnDistance,
                    planetCenter.z
                );
            }
        }
        
        Vector3 finalPosition = basePosition + spawnOffset;
        
        if (useRandomPosition && !spawnRelativeToPlanet)
        {
            // Add random offset within spawn radius (only if not already randomized by planet spawning)
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            finalPosition += new Vector3(randomCircle.x, randomCircle.y, 0);
        }
        
        return finalPosition;
    }
    
    int GetBallCount()
    {
        Ball[] ballsInScene = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        return ballsInScene.Length;
    }
    
    void ScheduleNextRandomSpawn()
    {
        float randomInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
        nextRandomSpawnTime = Time.time + randomInterval;
        Debug.Log($"Next random ball spawn scheduled in {randomInterval:F1} seconds");
    }
    
    void ScheduleNextMaintenanceCheck()
    {
        nextMaintenanceCheckTime = Time.time + maintenanceCheckInterval;
    }
    
    void CheckAndMaintainBallCount()
    {
        int currentBalls = GetBallCount();
        
        // Don't spawn if we're at or above max
        if (currentBalls >= maxBallsInScene)
        {
            return;
        }
        
        // Check if we should only spawn during gameplay
        if (onlySpawnDuringGameplay && gameManager != null)
        {
            if (gameManager.CurrentStage != GameStage.Playing)
            {
                return;
            }
        }
        
        bool needsUrgentSpawn = currentBalls <= lowBallThreshold;
        bool needsTargetMaintenance = currentBalls < targetBallCount;
        
        if (needsUrgentSpawn)
        {
            Debug.Log($"URGENT: Only {currentBalls} balls remaining (threshold: {lowBallThreshold}). Spawning immediately!");
            SpawnMaintenanceBall(true);
        }
        else if (needsTargetMaintenance)
        {
            Debug.Log($"Maintenance spawn: {currentBalls}/{targetBallCount} balls. Spawning to maintain target.");
            SpawnMaintenanceBall(false);
        }
    }
    
    GameObject SpawnMaintenanceBall(bool urgent)
    {
        // Use a short delay for urgent spawns, longer for regular maintenance
        float delay = urgent ? urgentSpawnDelay : urgentSpawnDelay * 2f;
        
        // Use random position for maintenance spawns to add variety
        bool originalRandomSetting = useRandomPosition;
        useRandomPosition = true;
        
        // Schedule the spawn with a small delay
        Invoke(nameof(SpawnMaintenanceBallDelayed), delay);
        
        // Restore original setting
        useRandomPosition = originalRandomSetting;
        
        return null; // Will be spawned with delay
    }
    
    void SpawnMaintenanceBallDelayed()
    {
        GameObject newBall = SpawnBall();
        if (newBall != null)
        {
            Debug.Log($"Maintenance ball spawned! Current count: {GetBallCount()}/{maxBallsInScene}");
            PlayMaintenanceSpawnEffects(newBall);
        }
    }
    
    void PlayMaintenanceSpawnEffects(GameObject newBall)
    {
        // Subtle effects for maintenance spawns (less dramatic than random spawns)
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayGoalEffect(newBall.transform.position, 0);
        }
        
        // No audio for maintenance spawns to avoid spam
        Debug.Log("Maintenance ball spawn effects triggered");
    }
    
    bool ShouldSpawnRandomBall()
    {
        // Check if we're at max balls
        if (limitMaxBalls && GetBallCount() >= maxBallsInScene)
        {
            Debug.Log($"Max balls ({maxBallsInScene}) reached, skipping random spawn");
            return false;
        }
        
        // Check if we should only spawn during gameplay
        if (onlySpawnDuringGameplay && gameManager != null)
        {
            if (gameManager.CurrentStage != GameStage.Playing)
            {
                Debug.Log("Not in gameplay stage, skipping random spawn");
                return false;
            }
        }
        
        return true;
    }
    
    GameObject SpawnRandomBall()
    {
        Debug.Log($"Random ball spawn triggered! Current balls: {GetBallCount()}/{maxBallsInScene}");
        
        // Use random position for random spawns
        bool originalRandomSetting = useRandomPosition;
        useRandomPosition = true;
        
        GameObject newBall = SpawnBall();
        
        // Restore original setting
        useRandomPosition = originalRandomSetting;
        
        if (newBall != null)
        {
            // Add some visual/audio feedback for random spawns
            PlayRandomSpawnEffects(newBall);
        }
        
        return newBall;
    }
    
    void PlayRandomSpawnEffects(GameObject newBall)
    {
        // Visual effects for random ball spawn
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayGoalEffect(newBall.transform.position, 0); // Special effect
        }
        
        // Audio feedback
        if (AudioManager.Instance != null)
        {
            // Play a special spawn sound - you'll need to add this to AudioManager
            // AudioManager.Instance.PlayBallSpawnSound();
        }
        
        Debug.Log("Random ball spawn effects triggered");
    }
    
    public void SpawnBallDelayed(float delay)
    {
        Invoke(nameof(SpawnBall), delay);
    }
    
    public void DestroyAllBalls()
    {
        Ball[] ballsInScene = FindObjectsByType<Ball>(FindObjectsSortMode.None);
        foreach (Ball ball in ballsInScene)
        {
            if (ball != null)
            {
                Destroy(ball.gameObject);
            }
        }
        Debug.Log($"Destroyed all {ballsInScene.Length} balls in scene");
    }
    
    // Public methods for external control
    public void EnableRandomSpawning(bool enable)
    {
        enableRandomSpawning = enable;
        if (enable)
        {
            ScheduleNextRandomSpawn();
        }
        Debug.Log($"Random spawning {(enable ? "enabled" : "disabled")}");
    }
    
    public void SetMaxBalls(int maxBalls)
    {
        maxBallsInScene = Mathf.Max(1, maxBalls);
        // Adjust target if it's higher than max
        targetBallCount = Mathf.Min(targetBallCount, maxBallsInScene - 1);
        Debug.Log($"Max balls set to: {maxBallsInScene}, target adjusted to: {targetBallCount}");
    }
    
    public void SetTargetBallCount(int target)
    {
        targetBallCount = Mathf.Clamp(target, 1, maxBallsInScene - 1);
        Debug.Log($"Target ball count set to: {targetBallCount}");
    }
    
    public void EnableMaintenanceSpawning(bool enable)
    {
        enableMaintenanceSpawning = enable;
        if (enable)
        {
            ScheduleNextMaintenanceCheck();
        }
        Debug.Log($"Maintenance spawning {(enable ? "enabled" : "disabled")}");
    }
    
    public void SetSpawnInterval(float minInterval, float maxInterval)
    {
        minSpawnInterval = Mathf.Max(1f, minInterval);
        maxSpawnInterval = Mathf.Max(minSpawnInterval, maxInterval);
        Debug.Log($"Spawn interval set to: {minSpawnInterval}-{maxSpawnInterval} seconds");
    }
    
    public void ForceSpawnBall()
    {
        Debug.Log("Force spawning ball...");
        SpawnRandomBall();
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

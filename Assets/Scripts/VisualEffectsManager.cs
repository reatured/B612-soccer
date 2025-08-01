using UnityEngine;

public class VisualEffectsManager : MonoBehaviour
{
    public static VisualEffectsManager Instance { get; private set; }
    
    [Header("Effect Prefabs")]
    public GameObject kickEffectPrefab;
    public GameObject goalEffectPrefab;
    public GameObject powerUpCollectEffectPrefab;
    public GameObject powerUpActivateEffectPrefab;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public void PlayKickEffect(Vector3 position, Vector3 direction)
    {
        if (kickEffectPrefab != null)
        {
            GameObject effect = Instantiate(kickEffectPrefab, position, Quaternion.LookRotation(direction));
            Destroy(effect, 2f);
        }
    }
    
    public void PlayGoalEffect(Vector3 position, int scoringPlayer)
    {
        if (goalEffectPrefab != null)
        {
            GameObject effect = Instantiate(goalEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 3f);
        }
    }
    
    public void PlayPowerUpCollectEffect(Vector3 position)
    {
        if (powerUpCollectEffectPrefab != null)
        {
            GameObject effect = Instantiate(powerUpCollectEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 2f);
        }
    }
    
    public void PlayPowerUpActivateEffect(Transform playerTransform)
    {
        if (powerUpActivateEffectPrefab != null && playerTransform != null)
        {
            GameObject effect = Instantiate(powerUpActivateEffectPrefab, playerTransform.position, Quaternion.identity);
            effect.transform.SetParent(playerTransform);
            Destroy(effect, 5f);
        }
    }
}
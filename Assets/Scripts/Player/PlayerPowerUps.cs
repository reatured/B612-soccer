using UnityEngine;
using System.Collections;

public class PlayerPowerUps : MonoBehaviour
{
    [Header("Power-ups")]
    public float powerKickMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float jumpMultiplier = 1f;
    public float powerUpDuration = 5f;
    
    private Coroutine currentPowerUpCoroutine;
    private PowerUpType? activePowerUp;
    
    public void ApplyPowerUp(PowerUpType powerUpType)
    {
        if (currentPowerUpCoroutine != null)
        {
            StopCoroutine(currentPowerUpCoroutine);
        }
        currentPowerUpCoroutine = StartCoroutine(PowerUpCoroutine(powerUpType));
    }
    
    private IEnumerator PowerUpCoroutine(PowerUpType powerUpType)
    {
        activePowerUp = powerUpType;
        
        switch (powerUpType)
        {
            case PowerUpType.PowerfulKick:
                powerKickMultiplier = 2f;
                break;
            case PowerUpType.BiggerBody:
                sizeMultiplier = 1.5f;
                transform.localScale = Vector3.one * sizeMultiplier;
                break;
            case PowerUpType.HigherJump:
                jumpMultiplier = 2f;
                break;
        }
        
        yield return new WaitForSeconds(powerUpDuration);
        
        ResetPowerUps();
    }
    
    private void ResetPowerUps()
    {
        powerKickMultiplier = 1f;
        sizeMultiplier = 1f;
        jumpMultiplier = 1f;
        transform.localScale = Vector3.one;
        activePowerUp = null;
        currentPowerUpCoroutine = null;
    }
    
    public bool HasActivePowerUp()
    {
        return activePowerUp.HasValue;
    }
    
    public PowerUpType? GetActivePowerUp()
    {
        return activePowerUp;
    }
    
    public float GetRemainingPowerUpTime()
    {
        return currentPowerUpCoroutine != null ? powerUpDuration : 0f;
    }
}
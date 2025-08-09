using UnityEngine;
using System.Collections;

public class PlayerAudio : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip kickBallSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip moveSound;
    
    [Header("Footstep Audio")]
    private bool isMoving = false;
    private bool isPlayingFootsteps = false;
    private Coroutine footstepCoroutine;
    
    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void PlayKickSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKickSound();
        }
        else
        {
            PlaySound(kickBallSound);
        }
    }
    
    public void PlayJumpSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }
        else
        {
            PlaySound(jumpSound);
        }
    }
    
    public void PlayLandSound()
    {
        PlaySound(landSound);
    }
    
    public void StartMovementAudio()
    {
        if (isPlayingFootsteps) return;
        
        isPlayingFootsteps = true;
        
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFootstepSound();
        }
    }
    
    public void StopMovementAudio()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
        }
        footstepCoroutine = StartCoroutine(StopFootstepsAfterDelay());
    }
    
    private IEnumerator StopFootstepsAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        isPlayingFootsteps = false;
        footstepCoroutine = null;
    }
    
    public void PlaySound(AudioClip clip)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public bool IsPlayingFootsteps()
    {
        return isPlayingFootsteps;
    }
}
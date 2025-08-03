using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    
    [Header("Volume Controls")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Range(0f, 1f)]
    public float sfxVolume = 1.0f;
    [Range(0f, 1f)]
    public float goalVolume = 1.0f;
    [Range(0f, 1f)]
    public float kickVolume = 1.0f;
    [Range(0f, 1f)]
    public float powerUpVolume = 1.0f;
    [Range(0f, 1f)]
    public float footstepVolume = 1.0f;
    [Range(0f, 1f)]
    public float jumpVolume = 1.0f;
    
    [Header("Audio Clips")]
    public AudioClip backgroundMusic;
    public AudioClip goalSound;
    public AudioClip powerUpSound;
    
    [Header("Audio Arrays")]
    public AudioClip[] kickSounds;
    public AudioClip[] footstepSounds;
    public AudioClip[] jumpSounds;
    
    [Header("Debug")]
    public string[] currentlyPlayingAudio;
    
    private List<string> playingAudioList = new List<string>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        if (musicSource == null)
            musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
            
        // Setup background music
        if (backgroundMusic != null && musicSource != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.Play();
            AddToDebugList($"Background Music: {backgroundMusic.name} (Looping)");
        }
    }
    
    public void PlayGoalSound()
    {
        if (goalSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(goalSound, goalVolume);
            AddToDebugList($"Goal: {goalSound.name}");
        }
    }
    
    public void PlayKickSound()
    {
        if (kickSounds != null && kickSounds.Length > 0 && sfxSource != null)
        {
            AudioClip randomKick = kickSounds[Random.Range(0, kickSounds.Length)];
            sfxSource.PlayOneShot(randomKick, kickVolume);
            AddToDebugList($"Kick: {randomKick.name}");
        }
    }
    
    public void PlayPowerUpSound()
    {
        if (powerUpSound != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(powerUpSound, powerUpVolume);
            AddToDebugList($"PowerUp: {powerUpSound.name}");
        }
    }
    
    public void PlayFootstepSound()
    {
        if (footstepSounds != null && footstepSounds.Length > 0 && sfxSource != null)
        {
            AudioClip randomFootstep = footstepSounds[Random.Range(0, footstepSounds.Length)];
            sfxSource.PlayOneShot(randomFootstep, footstepVolume);
            AddToDebugList($"Footstep: {randomFootstep.name}");
        }
    }
    
    public void PlayJumpSound()
    {
        if (jumpSounds != null && jumpSounds.Length > 0 && sfxSource != null)
        {
            AudioClip randomJump = jumpSounds[Random.Range(0, jumpSounds.Length)];
            sfxSource.PlayOneShot(randomJump, jumpVolume);
            AddToDebugList($"Jump: {randomJump.name}");
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip, sfxVolume);
            AddToDebugList($"SFX: {clip.name}");
        }
    }
    
    private void AddToDebugList(string audioName)
    {
        playingAudioList.Add($"{audioName} - {Time.time:F2}s");
        UpdateDebugArray();
        StartCoroutine(RemoveFromDebugListAfterDelay(audioName, 2f)); // Remove after 2 seconds
    }
    
    private void UpdateDebugArray()
    {
        currentlyPlayingAudio = playingAudioList.ToArray();
    }
    
    private System.Collections.IEnumerator RemoveFromDebugListAfterDelay(string audioName, float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = playingAudioList.Count - 1; i >= 0; i--)
        {
            if (playingAudioList[i].Contains(audioName))
            {
                playingAudioList.RemoveAt(i);
                break;
            }
        }
        UpdateDebugArray();
    }
}
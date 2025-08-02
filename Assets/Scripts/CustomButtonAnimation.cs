using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CustomButtonAnimation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Scale Animation Settings")]
    public bool enableHoverScale = true;
    [Range(0.5f, 2f)]
    public float hoverScale = 1.1f;
    public float hoverDuration = 0.2f;
    public AnimationCurve hoverCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Click Animation Settings")]
    public bool enableClickScale = true;
    [Range(0.5f, 2f)]
    public float clickScale = 0.95f;
    public float clickDuration = 0.1f;
    public AnimationCurve clickCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("Audio Settings")]
    public bool playHoverSound = false;
    public AudioClip hoverSound;
    public bool playClickSound = false;
    public AudioClip clickSound;
    [Range(0f, 1f)]
    public float audioVolume = 1f;
    
    [Header("Additional Effects")]
    public bool enableRotationWobble = false;
    [Range(0f, 30f)]
    public float wobbleAmount = 5f;
    public float wobbleSpeed = 2f;
    
    private Vector3 originalScale;
    private Coroutine currentScaleCoroutine;
    private Coroutine wobbleCoroutine;
    private AudioSource audioSource;
    private Button buttonComponent;
    private bool isHovering = false;
    private bool isPressed = false;
    
    void Start()
    {
        InitializeComponents();
    }
    
    void InitializeComponents()
    {
        originalScale = transform.localScale;
        buttonComponent = GetComponent<Button>();
        
        // Debug logging
        Debug.Log($"CustomButtonAnimation initialized on {gameObject.name}");
        Debug.Log($"Original scale: {originalScale}");
        Debug.Log($"Button component found: {buttonComponent != null}");
        
        // Check for Graphic Raycaster
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"GraphicRaycaster found: {raycaster != null}");
        }
        
        // Check if this object can receive raycast
        var graphic = GetComponent<Graphic>();
        if (graphic != null)
        {
            Debug.Log($"Raycast Target enabled: {graphic.raycastTarget}");
        }
        
        // Setup audio source if needed
        if (playHoverSound || playClickSound)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.volume = audioVolume;
            }
        }
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log($"OnPointerEnter called on {gameObject.name}");
        
        if (!enabled || (buttonComponent != null && !buttonComponent.interactable)) 
        {
            Debug.Log($"Animation blocked - Enabled: {enabled}, Interactable: {buttonComponent?.interactable}");
            return;
        }
        
        isHovering = true;
        
        if (enableHoverScale && !isPressed)
        {
            Debug.Log($"Starting hover animation - Scale: {originalScale * hoverScale}");
            AnimateScale(originalScale * hoverScale, hoverDuration, hoverCurve);
        }
        
        if (enableRotationWobble)
        {
            StartWobble();
        }
        
        if (playHoverSound && hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, audioVolume);
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!enabled) return;
        
        isHovering = false;
        
        if (enableHoverScale && !isPressed)
        {
            AnimateScale(originalScale, hoverDuration, hoverCurve);
        }
        
        if (enableRotationWobble)
        {
            StopWobble();
        }
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!enabled || (buttonComponent != null && !buttonComponent.interactable)) return;
        
        isPressed = true;
        
        if (enableClickScale)
        {
            AnimateScale(originalScale * clickScale, clickDuration, clickCurve);
        }
        
        if (playClickSound && clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound, audioVolume);
        }
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!enabled) return;
        
        isPressed = false;
        
        // Return to hover scale if still hovering, otherwise to original scale
        Vector3 targetScale = (enableHoverScale && isHovering) ? originalScale * hoverScale : originalScale;
        float duration = enableClickScale ? clickDuration : hoverDuration;
        AnimationCurve curve = enableClickScale ? clickCurve : hoverCurve;
        
        AnimateScale(targetScale, duration, curve);
    }
    
    void AnimateScale(Vector3 targetScale, float duration, AnimationCurve curve)
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        
        currentScaleCoroutine = StartCoroutine(ScaleAnimation(targetScale, duration, curve));
    }
    
    IEnumerator ScaleAnimation(Vector3 targetScale, float duration, AnimationCurve curve)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            float curveValue = curve.Evaluate(progress);
            
            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            yield return null;
        }
        
        transform.localScale = targetScale;
        currentScaleCoroutine = null;
    }
    
    void StartWobble()
    {
        if (wobbleCoroutine != null)
        {
            StopCoroutine(wobbleCoroutine);
        }
        
        wobbleCoroutine = StartCoroutine(WobbleAnimation());
    }
    
    void StopWobble()
    {
        if (wobbleCoroutine != null)
        {
            StopCoroutine(wobbleCoroutine);
            wobbleCoroutine = null;
        }
        
        // Return to original rotation
        StartCoroutine(ReturnToOriginalRotation());
    }
    
    IEnumerator WobbleAnimation()
    {
        Quaternion originalRotation = transform.rotation;
        
        while (isHovering)
        {
            float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
            transform.rotation = originalRotation * Quaternion.Euler(0, 0, wobble);
            yield return null;
        }
    }
    
    IEnumerator ReturnToOriginalRotation()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.identity;
        float duration = 0.2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, progress);
            yield return null;
        }
        
        transform.rotation = targetRotation;
    }
    
    void OnDisable()
    {
        // Reset to original state when disabled
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
            currentScaleCoroutine = null;
        }
        
        if (wobbleCoroutine != null)
        {
            StopCoroutine(wobbleCoroutine);
            wobbleCoroutine = null;
        }
        
        transform.localScale = originalScale;
        transform.rotation = Quaternion.identity;
        isHovering = false;
        isPressed = false;
    }
    
    // Public methods for external control
    public void ForceResetToOriginal()
    {
        if (currentScaleCoroutine != null)
        {
            StopCoroutine(currentScaleCoroutine);
        }
        
        AnimateScale(originalScale, hoverDuration, hoverCurve);
        isHovering = false;
        isPressed = false;
    }
    
    public void SetHoverScale(float newScale)
    {
        hoverScale = Mathf.Clamp(newScale, 0.5f, 2f);
    }
    
    public void SetClickScale(float newScale)
    {
        clickScale = Mathf.Clamp(newScale, 0.5f, 2f);
    }
    
    // Test methods for inspector
    [ContextMenu("Test Hover Animation")]
    void TestHoverAnimation()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Testing hover animation manually");
            AnimateScale(originalScale * hoverScale, hoverDuration, hoverCurve);
            StartCoroutine(TestResetAfterDelay());
        }
    }
    
    [ContextMenu("Debug Component State")]
    void DebugComponentState()
    {
        Debug.Log("=== CustomButtonAnimation Debug ===");
        Debug.Log($"GameObject: {gameObject.name}");
        Debug.Log($"Enabled: {enabled}");
        Debug.Log($"Original Scale: {originalScale}");
        Debug.Log($"Current Scale: {transform.localScale}");
        Debug.Log($"Enable Hover Scale: {enableHoverScale}");
        Debug.Log($"Hover Scale: {hoverScale}");
        Debug.Log($"Button Component: {buttonComponent}");
        Debug.Log($"Button Interactable: {buttonComponent?.interactable}");
        
        var graphic = GetComponent<Graphic>();
        Debug.Log($"Has Graphic: {graphic != null}");
        Debug.Log($"Raycast Target: {graphic?.raycastTarget}");
        
        Canvas canvas = GetComponentInParent<Canvas>();
        Debug.Log($"Canvas Found: {canvas != null}");
        if (canvas != null)
        {
            var raycaster = canvas.GetComponent<GraphicRaycaster>();
            Debug.Log($"GraphicRaycaster: {raycaster != null}");
        }
        
        var eventSystem = FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>();
        Debug.Log($"Event System Found: {eventSystem != null}");
    }
    
    IEnumerator TestResetAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        AnimateScale(originalScale, hoverDuration, hoverCurve);
    }
}
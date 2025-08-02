using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class UIFadeController : MonoBehaviour
{
    [Header("Fade Settings")]
    [Range(0.1f, 3f)]
    public float fadeInDuration = 0.5f;
    [Range(0.1f, 3f)]
    public float fadeOutDuration = 0.5f;
    
    [Header("Animation Curves")]
    public AnimationCurve fadeInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve fadeOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    
    [Header("Initial State")]
    public bool startVisible = true;
    
    private CanvasGroup canvasGroup;
    private Coroutine currentFadeCoroutine;
    private bool isVisible = true;
    
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        isVisible = startVisible;
        canvasGroup.alpha = startVisible ? 1f : 0f;
        
        if (!startVisible)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
    
    public void SetVisible(bool visible, bool immediate = false)
    {
        if (isVisible == visible && !immediate) return;
        
        isVisible = visible;
        
        if (immediate)
        {
            if (currentFadeCoroutine != null)
            {
                StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
            
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            return;
        }
        
        if (visible)
        {
            FadeIn();
        }
        else
        {
            FadeOut();
        }
    }
    
    public void FadeIn()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeAnimation(1f, fadeInDuration, fadeInCurve, true));
    }
    
    public void FadeOut()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }
        
        currentFadeCoroutine = StartCoroutine(FadeAnimation(0f, fadeOutDuration, fadeOutCurve, false));
    }
    
    private IEnumerator FadeAnimation(float targetAlpha, float duration, AnimationCurve curve, bool enableInteractionAtEnd)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsedTime = 0f;
        
        if (enableInteractionAtEnd)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            float curveValue = curve.Evaluate(progress);
            
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            yield return null;
        }
        
        canvasGroup.alpha = targetAlpha;
        
        if (!enableInteractionAtEnd)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        currentFadeCoroutine = null;
    }
    
    public void Toggle()
    {
        SetVisible(!isVisible);
    }
    
    public bool IsVisible()
    {
        return isVisible;
    }
    
    public bool IsFading()
    {
        return currentFadeCoroutine != null;
    }
    
    void OnDisable()
    {
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
            currentFadeCoroutine = null;
        }
    }
    
    [ContextMenu("Test Fade In")]
    void TestFadeIn()
    {
        if (Application.isPlaying)
        {
            FadeIn();
        }
    }
    
    [ContextMenu("Test Fade Out")]
    void TestFadeOut()
    {
        if (Application.isPlaying)
        {
            FadeOut();
        }
    }
}
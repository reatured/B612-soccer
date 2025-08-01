using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class StarTwinkle : MonoBehaviour
{
    [Header("Twinkle Settings")]
    public float twinkleSpeed = 2f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;
    
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;
    private float timeOffset;
    
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalAlpha = spriteRenderer.color.a;
        timeOffset = Random.Range(0f, Mathf.PI * 2f);
    }
    
    void Update()
    {
        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, 
                (Mathf.Sin(Time.time * twinkleSpeed + timeOffset) + 1f) * 0.5f);
            
            Color color = spriteRenderer.color;
            color.a = alpha * originalAlpha;
            spriteRenderer.color = color;
        }
    }
}
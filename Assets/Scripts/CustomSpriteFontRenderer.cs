using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class CustomSpriteFontRenderer : MonoBehaviour
{
    [Header("Font Settings")]
    public SpriteFont spriteFont;
    public string text = "";
    public Color color = Color.white;
    
    [Header("Layout Settings")]
    public float characterSpacing = 10f;
    public TextAnchor alignment = TextAnchor.MiddleCenter;
    public float scale = 1f;
    
    [Header("Character Size (assumes 512x512 sprites)")]
    public float characterSize = 64f;
    
    [Header("Auto Layout")]
    public bool autoSize = true;
    public float maxWidth = 200f;
    
    [Header("Gradient Settings")]
    public bool useGradient = false;
    public Color leftColor = Color.red;    // Player 1 (left of colon)
    public Color rightColor = Color.blue;  // Player 2 (right of colon)
    
    [Header("Flash Animation")]
    public bool yellowFlash = true;
    public Color flashColor = Color.yellow;
    public float flashDuration = 0.5f;
    
    private List<GameObject> characterObjects = new List<GameObject>();
    private RectTransform rectTransform;
    private string lastText = "";
    private Color lastColor = Color.white;
    private float lastScale = 1f;
    
    // Gradient and Flash state
    private int colonIndex = -1;
    private bool isFlashing = false;
    private List<Color> originalColors = new List<Color>();
    private bool hasBeenInitialized = false;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
    }
    
    void Start()
    {
        if (spriteFont == null)
        {
            Debug.LogWarning($"SpriteFont not assigned to {gameObject.name}");
            return;
        }
        
        // Only show default text if not empty and not controlled by GameManager
        if (!string.IsNullOrEmpty(text) && !hasBeenInitialized)
        {
            UpdateText();
            hasBeenInitialized = true;
        }
    }
    
    void Update()
    {
        if (HasTextChanged() || HasAppearanceChanged())
        {
            UpdateText();
        }
    }
    
    bool HasTextChanged()
    {
        return text != lastText;
    }
    
    bool HasAppearanceChanged()
    {
        return color != lastColor || scale != lastScale;
    }
    
    public void SetText(string newText)
    {
        text = newText;
        hasBeenInitialized = true;
        UpdateText();
    }
    
    public void SetColor(Color newColor)
    {
        color = newColor;
        UpdateText();
    }
    
    void UpdateText()
    {
        if (spriteFont == null) return;
        
        ClearCharacters();
        
        if (string.IsNullOrEmpty(text))
        {
            lastText = text;
            return;
        }
        
        CreateCharacters();
        LayoutCharacters();
        
        // Apply gradient or single color
        if (useGradient)
        {
            ApplyGradientColors();
        }
        else
        {
            ApplySingleColor();
        }
        
        lastText = text;
        lastColor = color;
        lastScale = scale;
    }
    
    void ClearCharacters()
    {
        // Clear character objects list
        foreach (GameObject obj in characterObjects)
        {
            if (obj != null)
            {
                if (Application.isPlaying)
                    Destroy(obj);
                else
                    DestroyImmediate(obj);
            }
        }
        characterObjects.Clear();
        
        // Safety check: destroy any remaining child objects with Image components
        Image[] childImages = GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.gameObject != gameObject) // Don't destroy the parent object
            {
                if (Application.isPlaying)
                    Destroy(img.gameObject);
                else
                    DestroyImmediate(img.gameObject);
            }
        }
        
        // Clear color lists
        originalColors.Clear();
    }
    
    void CreateCharacters()
    {
        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];
            Sprite characterSprite = spriteFont.GetSpriteForCharacter(character);
            
            if (characterSprite != null)
            {
                GameObject charObj = new GameObject($"Char_{character}_{i}");
                charObj.transform.SetParent(transform, false);
                
                Image charImage = charObj.AddComponent<Image>();
                charImage.sprite = characterSprite;
                charImage.color = color;
                charImage.preserveAspect = true;
                
                RectTransform charRect = charObj.GetComponent<RectTransform>();
                charRect.localScale = Vector3.one * scale;
                
                characterObjects.Add(charObj);
            }
            else if (character == ' ')
            {
                GameObject spaceObj = new GameObject($"Space_{i}");
                spaceObj.transform.SetParent(transform, false);
                
                RectTransform spaceRect = spaceObj.AddComponent<RectTransform>();
                spaceRect.sizeDelta = new Vector2(characterSize * scale, characterSize * scale);
                
                characterObjects.Add(spaceObj);
            }
            else
            {
                Debug.LogWarning($"No sprite found for character '{character}' in SpriteFont");
            }
        }
    }
    
    void LayoutCharacters()
    {
        if (characterObjects.Count == 0) return;
        
        float totalWidth = CalculateTotalWidth();
        Vector2 startPosition = GetStartPosition(totalWidth);
        
        float currentX = startPosition.x;
        float scaledCharSize = characterSize * scale;
        
        foreach (GameObject charObj in characterObjects)
        {
            RectTransform charRect = charObj.GetComponent<RectTransform>();
            
            // All characters use the same size (512x512 scaled)
            charRect.anchoredPosition = new Vector2(currentX + scaledCharSize * 0.5f, startPosition.y);
            charRect.sizeDelta = new Vector2(scaledCharSize, scaledCharSize);
            
            currentX += scaledCharSize + characterSpacing;
        }
        
        if (autoSize)
        {
            rectTransform.sizeDelta = new Vector2(totalWidth, scaledCharSize);
        }
    }
    
    float CalculateTotalWidth()
    {
        if (characterObjects.Count == 0) return 0f;
        
        float scaledCharSize = characterSize * scale;
        float totalWidth = characterObjects.Count * scaledCharSize;
        
        // Add spacing between characters (count - 1 spaces)
        if (characterObjects.Count > 1)
        {
            totalWidth += (characterObjects.Count - 1) * characterSpacing;
        }
        
        return totalWidth;
    }
    
    float GetMaxCharacterHeight()
    {
        // All characters are 512x512, so height is always the same
        return characterSize * scale;
    }
    
    Vector2 GetStartPosition(float totalWidth)
    {
        Vector2 position = Vector2.zero;
        
        // Horizontal alignment
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.MiddleLeft:
            case TextAnchor.LowerLeft:
                position.x = -rectTransform.sizeDelta.x * 0.5f;
                break;
                
            case TextAnchor.UpperCenter:
            case TextAnchor.MiddleCenter:
            case TextAnchor.LowerCenter:
                position.x = -totalWidth * 0.5f;
                break;
                
            case TextAnchor.UpperRight:
            case TextAnchor.MiddleRight:
            case TextAnchor.LowerRight:
                position.x = rectTransform.sizeDelta.x * 0.5f - totalWidth;
                break;
        }
        
        // Vertical alignment
        float scaledCharSize = characterSize * scale;
        switch (alignment)
        {
            case TextAnchor.UpperLeft:
            case TextAnchor.UpperCenter:
            case TextAnchor.UpperRight:
                position.y = rectTransform.sizeDelta.y * 0.5f - scaledCharSize * 0.5f;
                break;
                
            case TextAnchor.MiddleLeft:
            case TextAnchor.MiddleCenter:
            case TextAnchor.MiddleRight:
                position.y = 0f;
                break;
                
            case TextAnchor.LowerLeft:
            case TextAnchor.LowerCenter:
            case TextAnchor.LowerRight:
                position.y = -rectTransform.sizeDelta.y * 0.5f + scaledCharSize * 0.5f;
                break;
        }
        
        return position;
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            UpdateText();
        }
    }
    
    // === DEBUG AND TROUBLESHOOTING ===
    
    [ContextMenu("Debug Font Status")]
    void DebugFontStatus()
    {
        Debug.Log("=== CustomSpriteFontRenderer Debug ===");
        Debug.Log($"SpriteFont assigned: {spriteFont != null}");
        Debug.Log($"Text: '{text}'");
        Debug.Log($"Character objects count: {characterObjects.Count}");
        Debug.Log($"RectTransform: {rectTransform != null}");
        
        if (spriteFont != null)
        {
            Debug.Log($"Missing characters: '{spriteFont.GetMissingCharacters(text)}'");
            Debug.Log($"Has all characters: {spriteFont.HasAllCharacters(text)}");
        }
        
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (spriteFont != null)
            {
                Sprite sprite = spriteFont.GetSpriteForCharacter(c);
                Debug.Log($"Character '{c}' -> Sprite: {(sprite != null ? sprite.name : "NULL")}");
            }
        }
    }
    
    [ContextMenu("Force Update Text")]
    void ForceUpdateText()
    {
        UpdateText();
        Debug.Log("Forced text update completed");
    }
    
    // === GRADIENT AND FLASH SYSTEM ===
    
    int GetColonIndex()
    {
        if (string.IsNullOrEmpty(text)) return -1;
        return text.IndexOf(':');
    }
    
    bool IsCharacterOnLeftSide(int charIndex)
    {
        colonIndex = GetColonIndex();
        if (colonIndex == -1) return charIndex < text.Length / 2; // Fallback: split in middle
        return charIndex < colonIndex;
    }
    
    void ApplyGradientColors()
    {
        colonIndex = GetColonIndex();
        originalColors.Clear();
        
        for (int i = 0; i < characterObjects.Count; i++)
        {
            Image charImage = characterObjects[i].GetComponent<Image>();
            if (charImage != null)
            {
                Color gradientColor = GetGradientColorForCharacter(i);
                charImage.color = gradientColor;
                originalColors.Add(gradientColor);
            }
            else
            {
                originalColors.Add(Color.white);
            }
        }
    }
    
    void ApplySingleColor()
    {
        originalColors.Clear();
        
        foreach (GameObject charObj in characterObjects)
        {
            Image charImage = charObj.GetComponent<Image>();
            if (charImage != null)
            {
                charImage.color = color;
                originalColors.Add(color);
            }
            else
            {
                originalColors.Add(Color.white);
            }
        }
    }
    
    Color GetGradientColorForCharacter(int charIndex)
    {
        if (characterObjects.Count <= 1) return leftColor;
        
        // Calculate position ratio across the entire text
        float positionRatio = (float)charIndex / (characterObjects.Count - 1);
        
        // If we have a colon, adjust the gradient to be more pronounced on each side
        if (colonIndex != -1)
        {
            if (IsCharacterOnLeftSide(charIndex))
            {
                // Left side: interpolate from leftColor to a middle blend
                float leftRatio = colonIndex > 0 ? (float)charIndex / colonIndex : 0f;
                return Color.Lerp(leftColor, Color.Lerp(leftColor, rightColor, 0.3f), leftRatio);
            }
            else if (charIndex > colonIndex)
            {
                // Right side: interpolate from middle blend to rightColor
                float rightStart = colonIndex + 1;
                float rightLength = characterObjects.Count - rightStart;
                float rightRatio = rightLength > 0 ? (charIndex - rightStart) / rightLength : 1f;
                return Color.Lerp(Color.Lerp(leftColor, rightColor, 0.7f), rightColor, rightRatio);
            }
            else
            {
                // Colon character: blend of both colors
                return Color.Lerp(leftColor, rightColor, 0.5f);
            }
        }
        
        // No colon: simple left-to-right gradient
        return Color.Lerp(leftColor, rightColor, positionRatio);
    }
    
    // === FLASH ANIMATION SYSTEM ===
    
    public void TriggerFlash(int playerNumber)
    {
        if (!yellowFlash || isFlashing) return;
        
        StartCoroutine(FlashCoroutine(playerNumber == 1));
    }
    
    IEnumerator FlashCoroutine(bool leftSide)
    {
        isFlashing = true;
        
        // Determine which characters to flash
        List<int> flashIndices = new List<int>();
        for (int i = 0; i < text.Length; i++)
        {
            if (IsCharacterOnLeftSide(i) == leftSide)
            {
                flashIndices.Add(i);
            }
        }
        
        // Flash to target color
        yield return StartCoroutine(LerpFlashColors(flashIndices, flashColor, flashDuration * 0.5f));
        
        // Flash back to original color
        yield return StartCoroutine(LerpFlashColors(flashIndices, Color.clear, flashDuration * 0.5f));
        
        isFlashing = false;
    }
    
    IEnumerator LerpFlashColors(List<int> indices, Color targetColor, float duration)
    {
        float elapsed = 0f;
        List<Color> startColors = new List<Color>();
        
        // Record starting colors
        foreach (int index in indices)
        {
            if (index < characterObjects.Count)
            {
                Image charImage = characterObjects[index].GetComponent<Image>();
                if (charImage != null)
                {
                    startColors.Add(charImage.color);
                }
                else
                {
                    startColors.Add(Color.white);
                }
            }
        }
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            for (int i = 0; i < indices.Count; i++)
            {
                int charIndex = indices[i];
                if (charIndex < characterObjects.Count && i < startColors.Count)
                {
                    Image charImage = characterObjects[charIndex].GetComponent<Image>();
                    if (charImage != null)
                    {
                        Color endColor = targetColor == Color.clear ? 
                            (charIndex < originalColors.Count ? originalColors[charIndex] : Color.white) : 
                            targetColor;
                            
                        charImage.color = Color.Lerp(startColors[i], endColor, t);
                    }
                }
            }
            
            yield return null;
        }
        
        // Ensure final colors are set
        for (int i = 0; i < indices.Count; i++)
        {
            int charIndex = indices[i];
            if (charIndex < characterObjects.Count)
            {
                Image charImage = characterObjects[charIndex].GetComponent<Image>();
                if (charImage != null)
                {
                    Color finalColor = targetColor == Color.clear ? 
                        (charIndex < originalColors.Count ? originalColors[charIndex] : Color.white) : 
                        targetColor;
                        
                    charImage.color = finalColor;
                }
            }
        }
    }
}
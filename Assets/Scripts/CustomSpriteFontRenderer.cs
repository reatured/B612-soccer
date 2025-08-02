using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CustomSpriteFontRenderer : MonoBehaviour
{
    [Header("Font Settings")]
    public SpriteFont spriteFont;
    public string text = "0";
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
    
    private List<GameObject> characterObjects = new List<GameObject>();
    private RectTransform rectTransform;
    private string lastText = "";
    private Color lastColor = Color.white;
    private float lastScale = 1f;
    
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
        
        UpdateText();
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
        
        lastText = text;
        lastColor = color;
        lastScale = scale;
    }
    
    void ClearCharacters()
    {
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
}
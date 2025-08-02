using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Sprite Font", menuName = "UI/Sprite Font")]
public class SpriteFont : ScriptableObject
{
    [Header("Character Sprites")]
    [Tooltip("Sprites for digits 0-9")]
    public Sprite[] numberSprites = new Sprite[10];
    
    [Header("Special Characters")]
    [Tooltip("Sprite for colon ':' character")]
    public Sprite colonSprite;
    
    [Tooltip("Sprite for space character (optional, can use spaceWidth instead)")]
    public Sprite spaceSprite;
    
    [Header("Layout Settings (assumes 512x512 sprites)")]
    [Tooltip("Default character spacing between 512x512 sprites")]
    public float defaultSpacing = 10f;
    
    [Header("Character Mapping")]
    [Tooltip("Additional character-to-sprite mappings")]
    public CharacterSpriteMapping[] additionalMappings;
    
    private Dictionary<char, Sprite> characterMap;
    
    [System.Serializable]
    public class CharacterSpriteMapping
    {
        public char character;
        public Sprite sprite;
    }
    
    void OnEnable()
    {
        BuildCharacterMap();
    }
    
    void OnValidate()
    {
        BuildCharacterMap();
    }
    
    void BuildCharacterMap()
    {
        characterMap = new Dictionary<char, Sprite>();
        
        // Map number sprites (0-9)
        for (int i = 0; i < numberSprites.Length && i < 10; i++)
        {
            if (numberSprites[i] != null)
            {
                char digit = (char)('0' + i);
                characterMap[digit] = numberSprites[i];
            }
        }
        
        // Map special characters
        if (colonSprite != null)
        {
            characterMap[':'] = colonSprite;
        }
        
        if (spaceSprite != null)
        {
            characterMap[' '] = spaceSprite;
        }
        
        // Map additional characters
        if (additionalMappings != null)
        {
            foreach (var mapping in additionalMappings)
            {
                if (mapping.sprite != null)
                {
                    characterMap[mapping.character] = mapping.sprite;
                }
            }
        }
    }
    
    public Sprite GetSpriteForCharacter(char character)
    {
        if (characterMap == null)
        {
            BuildCharacterMap();
        }
        
        characterMap.TryGetValue(character, out Sprite sprite);
        return sprite;
    }
    
    public bool HasCharacter(char character)
    {
        if (characterMap == null)
        {
            BuildCharacterMap();
        }
        
        return characterMap.ContainsKey(character);
    }
    
    public bool HasAllCharacters(string text)
    {
        if (string.IsNullOrEmpty(text)) return true;
        
        foreach (char c in text)
        {
            if (c != ' ' && !HasCharacter(c))
            {
                return false;
            }
        }
        return true;
    }
    
    public string GetMissingCharacters(string text)
    {
        if (string.IsNullOrEmpty(text)) return "";
        
        System.Text.StringBuilder missing = new System.Text.StringBuilder();
        HashSet<char> addedChars = new HashSet<char>();
        
        foreach (char c in text)
        {
            if (c != ' ' && !HasCharacter(c) && !addedChars.Contains(c))
            {
                missing.Append(c);
                addedChars.Add(c);
            }
        }
        
        return missing.ToString();
    }
    
    public void AutoAssignNumberSprites(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0) return;
        
        // Auto-assign sprites based on sprite names if they contain numbers
        for (int i = 0; i < sprites.Length && i < 10; i++)
        {
            if (sprites[i] != null)
            {
                string spriteName = sprites[i].name.ToLower();
                
                // Try to find digit in sprite name
                for (int digit = 0; digit <= 9; digit++)
                {
                    if (spriteName.Contains(digit.ToString()))
                    {
                        if (digit < numberSprites.Length)
                        {
                            numberSprites[digit] = sprites[i];
                        }
                        break;
                    }
                }
            }
        }
        
        BuildCharacterMap();
    }
    
    #if UNITY_EDITOR
    [UnityEditor.MenuItem("Assets/Create/UI/Auto-Setup Sprite Font from Selection")]
    static void CreateSpriteFontFromSelection()
    {
        Object[] selectedObjects = UnityEditor.Selection.objects;
        Sprite[] sprites = System.Array.ConvertAll(selectedObjects, obj => obj as Sprite);
        sprites = System.Array.FindAll(sprites, sprite => sprite != null);
        
        if (sprites.Length == 0)
        {
            Debug.LogWarning("No sprites selected. Please select sprite assets to create a SpriteFont.");
            return;
        }
        
        // Create new SpriteFont asset
        SpriteFont spriteFont = CreateInstance<SpriteFont>();
        spriteFont.AutoAssignNumberSprites(sprites);
        
        string path = UnityEditor.AssetDatabase.GetAssetPath(sprites[0]);
        string directory = System.IO.Path.GetDirectoryName(path);
        string fontPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath($"{directory}/CustomSpriteFont.asset");
        
        UnityEditor.AssetDatabase.CreateAsset(spriteFont, fontPath);
        UnityEditor.AssetDatabase.SaveAssets();
        
        UnityEditor.Selection.activeObject = spriteFont;
        Debug.Log($"Created SpriteFont asset at {fontPath} with {sprites.Length} sprites");
    }
    #endif
}
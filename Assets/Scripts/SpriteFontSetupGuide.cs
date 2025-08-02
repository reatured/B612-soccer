using UnityEngine;

public class SpriteFontSetupGuide : MonoBehaviour
{
    /*
     * CUSTOM SPRITE FONT SETUP GUIDE
     * ==============================
     * 
     * This guide helps you set up your custom sketched number font system.
     * 
     * STEP 1: PREPARE YOUR NUMBER IMAGE (512x512 per character)
     * ----------------------------------------------------------
     * 1. Import your sketched numbers image into Assets/Font/ folder
     * 2. Set Texture Type to "Sprite (2D and UI)"
     * 3. Set Sprite Mode to "Multiple"
     * 4. Click "Sprite Editor" button
     * 5. Create 512x512 rectangles for each number 0-9
     *    - Each character should be exactly 512x512 pixels
     *    - This ensures equal spacing and alignment
     * 6. Name each sprite slice: "0", "1", "2", ..., "9"
     * 7. Optionally create a 512x512 colon ":" sprite
     * 8. Apply changes
     * 
     * STEP 2: CREATE SPRITE FONT ASSET
     * --------------------------------
     * Option A - Automatic (Recommended):
     * 1. Select all your number sprites (0-9) in the Project window
     * 2. Right-click and choose "Create > UI > Auto-Setup Sprite Font from Selection"
     * 3. This will create a SpriteFont asset and auto-assign your sprites
     * 
     * Option B - Manual:
     * 1. Right-click in Project window
     * 2. Choose "Create > UI > Sprite Font"
     * 3. Assign each number sprite (0-9) to the corresponding slot
     * 4. Optionally assign a colon ":" sprite for timer display
     * 
     * STEP 3: UPDATE UI COMPONENTS
     * ---------------------------
     * 1. In your scene, find UI Text components that show scores/timer
     * 2. Replace TextMeshPro components with CustomSpriteFontRenderer:
     *    - Remove the TextMeshPro component
     *    - Add CustomSpriteFontRenderer component
     *    - Assign your SpriteFont asset to the "Sprite Font" field
     * 
     * STEP 4: UPDATE GAMEMANAGER REFERENCES
     * ------------------------------------
     * The GameManager has been updated to use CustomSpriteFontRenderer:
     * - combinedScoreText: Shows score as "0 : 0"
     * - timerText: Shows timer as "00:00"
     * 
     * Drag your CustomSpriteFontRenderer components to these fields in GameManager.
     * 
     * CUSTOMIZATION OPTIONS
     * ====================
     * 
     * CustomSpriteFontRenderer Properties:
     * - Sprite Font: Your SpriteFont asset
     * - Text: The text to display
     * - Color: Tint color (supports timer warnings: white/yellow/red)
     * - Character Spacing: Space between 512x512 characters
     * - Character Size: Display size of each 512x512 character (default: 64)
     * - Alignment: Text alignment (Left, Center, Right, etc.)
     * - Scale: Size multiplier for all characters
     * - Auto Size: Automatically resize component to fit text
     * - Max Width: Maximum width for auto-sizing
     * 
     * SpriteFont Properties:
     * - Number Sprites: 512x512 sprites for digits 0-9
     * - Colon Sprite: 512x512 sprite for ":" character
     * - Space Sprite: Optional 512x512 sprite for space characters
     * - Default Spacing: Default spacing between characters
     * - Additional Mappings: Map custom characters to 512x512 sprites
     * 
     * ADVANCED FEATURES
     * ================
     * 
     * 1. Adding Custom Characters:
     *    - Create sprites for additional characters (-, +, %, etc.)
     *    - Add them to "Additional Mappings" in your SpriteFont
     * 
     * 2. Multiple Font Styles:
     *    - Create different SpriteFont assets for different styles
     *    - Switch fonts at runtime by changing the spriteFont reference
     * 
     * 3. Runtime Text Changes:
     *    - Use SetText("new text") to change displayed text
     *    - Use SetColor(Color.red) to change color
     * 
     * TROUBLESHOOTING
     * ==============
     * 
     * Problem: Characters appear as missing
     * Solution: Check that sprites are properly assigned in SpriteFont
     * 
     * Problem: Text doesn't update
     * Solution: Ensure you're calling SetText() instead of directly setting text property
     * 
     * Problem: Colors don't change for timer warnings
     * Solution: Make sure you're using SetColor() in timer update code
     * 
     * Problem: Spacing looks wrong
     * Solution: Adjust Character Spacing or Scale in CustomSpriteFontRenderer
     * 
     * PERFORMANCE TIPS
     * ===============
     * 
     * 1. Use fewer sprite objects for better performance
     * 2. Disable "Auto Size" if you don't need dynamic sizing
     * 3. Set specific width/height instead of using auto-sizing
     * 4. Pool character objects if creating/destroying text frequently
     * 
     */
    
    [Header("Quick Setup")]
    [Tooltip("Drag your number sprites here, then click 'Auto Create Font'")]
    public Sprite[] numberSprites = new Sprite[10];
    
    [Tooltip("Optional: Sprite for colon character")]
    public Sprite colonSprite;
    
    [ContextMenu("Auto Create Font")]
    void AutoCreateFont()
    {
        if (numberSprites == null || numberSprites.Length == 0)
        {
            Debug.LogError("Please assign number sprites first!");
            return;
        }
        
        // Create SpriteFont asset
        SpriteFont font = ScriptableObject.CreateInstance<SpriteFont>();
        
        // Assign number sprites
        for (int i = 0; i < numberSprites.Length && i < 10; i++)
        {
            if (numberSprites[i] != null)
            {
                font.numberSprites[i] = numberSprites[i];
            }
        }
        
        // Assign colon sprite
        if (colonSprite != null)
        {
            font.colonSprite = colonSprite;
        }
        
        #if UNITY_EDITOR
        // Save asset
        UnityEditor.AssetDatabase.CreateAsset(font, "Assets/Font/CustomSpriteFont.asset");
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.Selection.activeObject = font;
        
        Debug.Log("Created SpriteFont asset at Assets/Font/CustomSpriteFont.asset");
        #endif
    }
    
    [ContextMenu("Show Setup Instructions")]
    void ShowInstructions()
    {
        Debug.Log("Check the SpriteFontSetupGuide script for detailed setup instructions!");
    }
}
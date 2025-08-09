using UnityEngine;

public enum InputSource
{
    Human,      // Keyboard/controller input
    AI,         // Behavior tree commands
    Hybrid,     // Mix of human and AI (future use)
    None        // No input (spectator mode)
}

public class PlayerInput : MonoBehaviour
{
    [Header("Input Source")]
    public InputSource inputSource = InputSource.Human;
    
    [Header("Human Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    
    [Header("AI Input Settings")]
    public bool allowAIOverride = false;
    public float aiInputSmoothTime = 0.1f;
    
    private Player player;
    
    // Public input state
    public float MoveInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool JumpHeld { get; private set; }
    
    // AI input injection
    private float aiMoveInput = 0f;
    private bool aiJumpPressed = false;
    private bool aiJumpHeld = false;
    private float aiMoveInputVelocity = 0f;
    
    // Input smoothing
    private float smoothedMoveInput = 0f;
    
    void Awake()
    {
        player = GetComponent<Player>();
        SetupPlayerKeys();
    }
    
    void SetupPlayerKeys()
    {
        if (player.playerNumber == 1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;
        }
        else if (player.playerNumber == 2)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
        }
    }
    
    void Update()
    {
        UpdateInput();
    }
    
    void UpdateInput()
    {
        switch (inputSource)
        {
            case InputSource.Human:
                UpdateHumanInput();
                break;
            case InputSource.AI:
                UpdateAIInput();
                break;
            case InputSource.Hybrid:
                UpdateHybridInput();
                break;
            case InputSource.None:
                UpdateNoInput();
                break;
        }
    }
    
    void UpdateHumanInput()
    {
        float moveInput = 0f;
        
        if (Input.GetKey(leftKey))
        {
            moveInput = -1f;
        }
        if (Input.GetKey(rightKey))
        {
            moveInput = 1f;
        }
        
        if (player.playerNumber == 2)
            moveInput = -moveInput;
        
        MoveInput = moveInput;
        JumpPressed = Input.GetKeyDown(jumpKey);
        JumpHeld = Input.GetKey(jumpKey);
    }
    
    void UpdateAIInput()
    {
        // Smooth AI movement input for more natural movement
        smoothedMoveInput = Mathf.SmoothDamp(smoothedMoveInput, aiMoveInput, ref aiMoveInputVelocity, aiInputSmoothTime);
        
        MoveInput = smoothedMoveInput;
        JumpPressed = aiJumpPressed;
        JumpHeld = aiJumpHeld;
        
        // Reset AI jump pressed after one frame (like GetKeyDown)
        aiJumpPressed = false;
    }
    
    void UpdateHybridInput()
    {
        // Start with human input
        UpdateHumanInput();
        
        // Allow AI to override if enabled
        if (allowAIOverride)
        {
            if (Mathf.Abs(aiMoveInput) > 0.1f)
                MoveInput = aiMoveInput;
            
            if (aiJumpPressed)
                JumpPressed = true;
                
            if (aiJumpHeld)
                JumpHeld = true;
        }
        
        // Reset AI inputs
        aiJumpPressed = false;
    }
    
    void UpdateNoInput()
    {
        MoveInput = 0f;
        JumpPressed = false;
        JumpHeld = false;
    }
    
    public bool IsMoving()
    {
        return Mathf.Abs(MoveInput) > 0.1f;
    }
    
    public Vector2 GetMoveDirection()
    {
        return new Vector2(MoveInput, 0f);
    }
    
    // AI Input Injection Methods
    public void SetAIMove(float moveDirection)
    {
        aiMoveInput = Mathf.Clamp(moveDirection, -1f, 1f);
    }
    
    public void SetAIJump(bool pressed, bool held = false)
    {
        aiJumpPressed = pressed;
        aiJumpHeld = held;
    }
    
    public void TriggerAIJump()
    {
        aiJumpPressed = true;
        aiJumpHeld = false;
    }
    
    // Input Source Management
    public void SetInputSource(InputSource source)
    {
        inputSource = source;
        
        // Reset AI inputs when switching away from AI mode
        if (source != InputSource.AI && source != InputSource.Hybrid)
        {
            ResetAIInputs();
        }
    }
    
    public void SwitchToHuman()
    {
        SetInputSource(InputSource.Human);
    }
    
    public void SwitchToAI()
    {
        SetInputSource(InputSource.AI);
    }
    
    public void EnableSpectatorMode()
    {
        SetInputSource(InputSource.None);
    }
    
    void ResetAIInputs()
    {
        aiMoveInput = 0f;
        aiJumpPressed = false;
        aiJumpHeld = false;
        smoothedMoveInput = 0f;
        aiMoveInputVelocity = 0f;
    }
    
    // Utility Methods
    public bool IsHumanControlled()
    {
        return inputSource == InputSource.Human || inputSource == InputSource.Hybrid;
    }
    
    public bool IsAIControlled()
    {
        return inputSource == InputSource.AI || inputSource == InputSource.Hybrid;
    }
    
    public string GetInputSourceName()
    {
        return inputSource.ToString();
    }
}
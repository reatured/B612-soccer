using UnityEngine;
using StateMachine;

[RequireComponent(typeof(PlayerStateMachine))]
public class PlayerStateDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    public bool showStateInfo = true;
    public bool logStateChanges = true;
    public bool showGizmos = true;
    public bool showAIDebug = true;
    
    [Header("GUI Display")]
    public bool showOnScreenDebug = true;
    public float debugGUIScale = 1f;
    public Color debugTextColor = Color.white;
    public Color aiDebugColor = Color.cyan;
    
    private PlayerStateMachine stateMachine;
    private Player player;
    private SoccerAI soccerAI;
    private string previousStateName = "";
    
    void Awake()
    {
        stateMachine = GetComponent<PlayerStateMachine>();
        player = GetComponent<Player>();
        soccerAI = GetComponent<SoccerAI>();
    }
    
    void Update()
    {
        if (logStateChanges && stateMachine != null)
        {
            string currentStateName = stateMachine.currentStateName;
            if (currentStateName != previousStateName)
            {
                Debug.Log($"[Player {player.playerNumber}] State changed: {previousStateName} → {currentStateName}");
                previousStateName = currentStateName;
            }
        }
    }
    
    void OnGUI()
    {
        if (!showOnScreenDebug || stateMachine == null) return;
        
        // Calculate position based on player number
        float xPos = player.playerNumber == 1 ? 10f : Screen.width - 300f;
        float yPos = 10f;
        
        // Scale GUI
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(debugGUIScale, debugGUIScale, 1f));
        
        // Adjust position for scale
        xPos /= debugGUIScale;
        yPos /= debugGUIScale;
        
        // Set text color
        GUI.color = debugTextColor;
        
        // Display player info
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"=== Player {player.playerNumber} Debug ===");
        yPos += 25f;
        
        // State machine info
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Current State: {stateMachine.currentStateName}");
        yPos += 20f;
        
        // Movement info
        if (stateMachine.movement != null)
        {
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Grounded: {stateMachine.movement.isGrounded}");
            yPos += 20f;
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Facing Right: {stateMachine.movement.facingRight}");
            yPos += 20f;
        }
        
        // Input info
        if (stateMachine.input != null)
        {
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Move Input: {stateMachine.input.MoveInput:F2}");
            yPos += 20f;
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Jump Pressed: {stateMachine.input.JumpPressed}");
            yPos += 20f;
        }
        
        // Power-up info
        if (stateMachine.powerUps != null)
        {
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Has PowerUp: {stateMachine.powerUps.HasActivePowerUp()}");
            yPos += 20f;
            if (stateMachine.powerUps.HasActivePowerUp())
            {
                GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"PowerUp Type: {stateMachine.powerUps.GetActivePowerUp()}");
                yPos += 20f;
            }
        }
        
        // Architecture settings
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"State Machine: {player.useStateMachine}");
        yPos += 20f;
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Behavior Tree: {player.useBehaviorTree}");
        yPos += 20f;
        
        // Player Type and AI info
        GUI.color = aiDebugColor;
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Type: {player.GetPlayerTypeDescription()}");
        yPos += 20f;
        GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Input Source: {stateMachine.input.GetInputSourceName()}");
        yPos += 20f;
        
        // AI-specific debug info
        if (showAIDebug && player.IsAI() && soccerAI != null)
        {
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"=== AI Debug ===");
            yPos += 20f;
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Skill Level: {player.GetAISkillLevel():F2}");
            yPos += 20f;
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Reaction Time: {player.GetAIReactionTime():F2}s");
            yPos += 20f;
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Tactical State: {soccerAI.GetTacticalState():F2}");
            yPos += 20f;
            
            Vector2 optimalPos = soccerAI.GetOptimalPosition();
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Target Pos: ({optimalPos.x:F1}, {optimalPos.y:F1})");
            yPos += 20f;
            
            Vector2 predictedBall = soccerAI.GetPredictedBallPosition();
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Ball Pred: ({predictedBall.x:F1}, {predictedBall.y:F1})");
            yPos += 20f;
            
            string aiActions = "";
            if (soccerAI.ShouldChaseAggressively()) aiActions += "Chase ";
            if (soccerAI.ShouldDefend()) aiActions += "Defend ";
            if (soccerAI.ShouldPositionForIntercept()) aiActions += "Intercept ";
            if (soccerAI.ShouldJumpForBall()) aiActions += "Jump ";
            
            GUI.Label(new Rect(xPos, yPos, 280f, 20f), $"Actions: {aiActions}");
        }
        
        GUI.matrix = oldMatrix;
        GUI.color = Color.white;
    }
    
    void OnDrawGizmos()
    {
        if (!showGizmos || stateMachine == null) return;
        
        // Draw state-specific gizmos
        Gizmos.color = player.playerNumber == 1 ? Color.cyan : Color.magenta;
        
        // Show current state visually
        Vector3 textPos = transform.position + Vector3.up * 2f;
        
#if UNITY_EDITOR
        UnityEditor.Handles.Label(textPos, $"P{player.playerNumber}: {stateMachine.currentStateName}");
#endif
        
        // Draw state-specific visualizations
        if (stateMachine.movement != null)
        {
            // Ground check visualization
            Gizmos.color = stateMachine.movement.isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            
            // Movement direction
            if (stateMachine.input != null && stateMachine.input.IsMoving())
            {
                Gizmos.color = Color.yellow;
                Vector3 moveDir = new Vector3(stateMachine.input.MoveInput, 0f, 0f);
                Gizmos.DrawRay(transform.position, moveDir * 2f);
            }
        }
    }
}
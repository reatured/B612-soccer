using UnityEngine;
using BehaviorTree;

[RequireComponent(typeof(PlayerBehaviorTreeRunner))]
public class BehaviorTreeDebugger : MonoBehaviour
{
    [Header("Behavior Tree Debug")]
    public bool showBTInfo = true;
    public bool logBTExecution = false;
    public bool showNodeStates = true;
    
    [Header("Visual Debug")]
    public bool showOnScreenBT = true;
    public float btGUIScale = 0.8f;
    public Color successColor = Color.green;
    public Color failureColor = Color.red;
    public Color runningColor = Color.yellow;
    
    private PlayerBehaviorTreeRunner btRunner;
    private Player player;
    
    void Awake()
    {
        btRunner = GetComponent<PlayerBehaviorTreeRunner>();
        player = GetComponent<Player>();
    }
    
    void OnGUI()
    {
        if (!showOnScreenBT || btRunner == null || !player.useBehaviorTree) return;
        
        BTNode rootNode = btRunner.GetRootNode();
        if (rootNode == null) return;
        
        // Position based on player number
        float xPos = player.playerNumber == 1 ? 10f : Screen.width - 350f;
        float yPos = 200f; // Below the state machine debug
        
        // Scale GUI
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(btGUIScale, btGUIScale, 1f));
        
        xPos /= btGUIScale;
        yPos /= btGUIScale;
        
        // Display BT info
        GUI.color = Color.cyan;
        GUI.Label(new Rect(xPos, yPos, 330f, 20f), $"=== Player {player.playerNumber} Behavior Tree ===");
        yPos += 25f;
        
        // Draw tree structure recursively
        DrawNodeGUI(rootNode, xPos, ref yPos, 0);
        
        GUI.matrix = oldMatrix;
        GUI.color = Color.white;
    }
    
    void DrawNodeGUI(BTNode node, float xPos, ref float yPos, int depth)
    {
        if (node == null) return;
        
        // Indent based on depth
        float indentedXPos = xPos + (depth * 20f);
        
        // Choose color based on node state
        Color nodeColor = GetStateColor(node.state);
        GUI.color = nodeColor;
        
        // Display node info
        string nodeInfo = $"{GetIndentString(depth)}{node.name} [{node.state}]";
        GUI.Label(new Rect(indentedXPos, yPos, 300f, 20f), nodeInfo);
        yPos += 18f;
        
        // Draw children for composite nodes
        if (node is BTComposite composite)
        {
            var childrenField = typeof(BTComposite).GetField("children", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (childrenField != null)
            {
                var children = childrenField.GetValue(composite) as System.Collections.Generic.List<BTNode>;
                if (children != null)
                {
                    foreach (var child in children)
                    {
                        DrawNodeGUI(child, xPos, ref yPos, depth + 1);
                    }
                }
            }
        }
        else if (node is BTDecorator decorator)
        {
            var childField = typeof(BTDecorator).GetField("child", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (childField != null)
            {
                var child = childField.GetValue(decorator) as BTNode;
                if (child != null)
                {
                    DrawNodeGUI(child, xPos, ref yPos, depth + 1);
                }
            }
        }
    }
    
    Color GetStateColor(BTNodeState state)
    {
        switch (state)
        {
            case BTNodeState.Success: return successColor;
            case BTNodeState.Failure: return failureColor;
            case BTNodeState.Running: return runningColor;
            default: return Color.white;
        }
    }
    
    string GetIndentString(int depth)
    {
        string indent = "";
        for (int i = 0; i < depth; i++)
        {
            indent += "  ";
        }
        return indent;
    }
    
    void OnDrawGizmos()
    {
        if (!showBTInfo || btRunner == null || !player.useBehaviorTree) return;
        
        // Draw BT execution status
        Gizmos.color = Color.cyan;
        Vector3 btTextPos = transform.position + Vector3.up * 3f;
        
#if UNITY_EDITOR
        BTNode rootNode = btRunner.GetRootNode();
        if (rootNode != null)
        {
            UnityEditor.Handles.Label(btTextPos, $"BT: {rootNode.state}");
        }
#endif
    }
}
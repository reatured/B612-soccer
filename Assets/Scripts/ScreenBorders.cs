using UnityEngine;

public class ScreenBorders : MonoBehaviour
{
    [Header("Border Settings")]
    public float borderWidth = 1f;
    public float borderOffset = 2f;
    
    [Header("Physics")]
    public PhysicsMaterial2D borderMaterial;
    
    private Camera mainCamera;
    private GameObject[] borders;
    
    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        CreateBorders();
    }
    
    void CreateBorders()
    {
        if (mainCamera == null)
        {
            Debug.LogError("No camera found for creating borders!");
            return;
        }
        
        Vector3 bottomLeft = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, mainCamera.nearClipPlane));
        
        float screenWidth = topRight.x - bottomLeft.x;
        float screenHeight = topRight.y - bottomLeft.y;
        
        borders = new GameObject[4];
        
        borders[0] = CreateBorder("TopBorder", 
            new Vector3(0, topRight.y + borderOffset + borderWidth/2, 0), 
            new Vector2(screenWidth + borderOffset*2 + borderWidth*2, borderWidth));
            
        borders[1] = CreateBorder("BottomBorder", 
            new Vector3(0, bottomLeft.y - borderOffset - borderWidth/2, 0), 
            new Vector2(screenWidth + borderOffset*2 + borderWidth*2, borderWidth));
            
        borders[2] = CreateBorder("LeftBorder", 
            new Vector3(bottomLeft.x - borderOffset - borderWidth/2, 0, 0), 
            new Vector2(borderWidth, screenHeight + borderOffset*2 + borderWidth*2));
            
        borders[3] = CreateBorder("RightBorder", 
            new Vector3(topRight.x + borderOffset + borderWidth/2, 0, 0), 
            new Vector2(borderWidth, screenHeight + borderOffset*2 + borderWidth*2));
        
        Debug.Log($"Created screen borders. Screen size: {screenWidth}x{screenHeight}");
    }
    
    GameObject CreateBorder(string name, Vector3 position, Vector2 size)
    {
        GameObject border = new GameObject(name);
        border.transform.parent = transform;
        border.transform.position = position;
        
        BoxCollider2D collider = border.AddComponent<BoxCollider2D>();
        collider.size = size;
        
        if (borderMaterial != null)
        {
            collider.sharedMaterial = borderMaterial;
        }
        else
        {
            PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D("BorderMaterial");
            bouncyMaterial.bounciness = 1f;
            bouncyMaterial.friction = 0f;
            collider.sharedMaterial = bouncyMaterial;
        }
        
        border.layer = gameObject.layer;
        
        return border;
    }
    
    void OnDrawGizmos()
    {
        if (borders == null) return;
        
        Gizmos.color = Color.red;
        
        foreach (GameObject border in borders)
        {
            if (border != null)
            {
                BoxCollider2D collider = border.GetComponent<BoxCollider2D>();
                if (collider != null)
                {
                    Gizmos.DrawWireCube(border.transform.position, collider.size);
                }
            }
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            RecreateBoders();
        }
    }
    
    public void RecreateBoders()
    {
        DestroyBorders();
        CreateBorders();
    }
    
    void DestroyBorders()
    {
        if (borders != null)
        {
            foreach (GameObject border in borders)
            {
                if (border != null)
                {
                    DestroyImmediate(border);
                }
            }
        }
    }
    
    void OnDestroy()
    {
        DestroyBorders();
    }
}
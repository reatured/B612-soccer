using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Planet Properties")]
    public float radius = 5f;
    public Transform center;
    
    [Header("Player Setup")]
    public Transform player1;
    public Transform player2;
    public float playerOffset = 0.5f;
    
    void Start()
    {
        if (center == null)
            center = transform;
            
        SetupPlayers();
    }
    
    void SetupPlayers()
    {
        if (player1 != null)
        {
            PositionPlayerOnPlanet(player1, Mathf.PI);
        }
        
        if (player2 != null)
        {
            PositionPlayerOnPlanet(player2, 0f);
        }
    }
    
    public void PositionPlayerOnPlanet(Transform player, float angle)
    {
        Vector2 centerPos = center.position;
        float surfaceRadius = radius + playerOffset;
        
        Vector2 position = new Vector2(
            centerPos.x + Mathf.Cos(angle) * surfaceRadius,
            centerPos.y + Mathf.Sin(angle) * surfaceRadius
        );
        
        player.position = position;
        
        Vector2 surfaceNormal = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        float rotationAngle = Mathf.Atan2(surfaceNormal.y, surfaceNormal.x) * Mathf.Rad2Deg + 90f;
        player.rotation = Quaternion.AngleAxis(rotationAngle, Vector3.forward);
    }
    
    public float GetAngleFromPosition(Vector2 position)
    {
        Vector2 centerPos = center.position;
        Vector2 direction = position - centerPos;
        return Mathf.Atan2(direction.y, direction.x);
    }
    
    public Vector2 GetSurfacePosition(float angle)
    {
        Vector2 centerPos = center.position;
        return new Vector2(
            centerPos.x + Mathf.Cos(angle) * (radius + playerOffset),
            centerPos.y + Mathf.Sin(angle) * (radius + playerOffset)
        );
    }
    
    void OnDrawGizmos()
    {
        if (center == null) center = transform;
        
        Gizmos.color = Color.white;
        DrawWireCircle(center.position, radius);
        
        Gizmos.color = Color.yellow;
        DrawWireCircle(center.position, radius + playerOffset);
    }
    
    void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 64;
        float angleStep = 2f * Mathf.PI / segments;
        Vector3 prevPoint = center + new Vector3(Mathf.Cos(0) * radius, Mathf.Sin(0) * radius, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}

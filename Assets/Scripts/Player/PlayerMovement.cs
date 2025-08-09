using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    public float jumpForce = 8f;
    public float jumpMultiplier = 1f;
    
    [Header("Ground Check")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 1.0f;
    public float groundCheckRadius = 0.3f;
    
    private Planet planet;
    private Rigidbody2D rb;
    private Player player;
    public bool isGrounded { get; private set; }
    public bool facingRight { get; private set; } = true;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        planet = FindFirstObjectByType<Planet>();
    }
    
    public void CheckGrounded()
    {
        if (planet == null) return;
        
        float distanceToCenter = Vector2.Distance(transform.position, planet.center.position);
        float surfaceDistance = distanceToCenter - planet.radius;
        
        bool wasGrounded = isGrounded;
        isGrounded = surfaceDistance <= groundCheckDistance;
        
        if (!wasGrounded && isGrounded)
        {
            rb.linearVelocity *= 0.8f;
        }
    }
    
    public void MoveAroundPlanet(float direction)
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 tangentDirection;
        
        if (player.playerNumber == 1)
        {
            tangentDirection = new Vector2(-directionToPlanet.y, directionToPlanet.x) * direction;
        }
        else
        {
            tangentDirection = new Vector2(directionToPlanet.y, -directionToPlanet.x) * direction;
        }
        
        Vector2 targetVelocity = tangentDirection * moveSpeed;
        
        if (isGrounded)
        {
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 gravityDirection = directionToPlanet;
            float radialVelocity = Vector2.Dot(currentVelocity, gravityDirection);
            Vector2 radialComponent = gravityDirection * radialVelocity;
            
            rb.linearVelocity = targetVelocity + radialComponent;
        }
        else
        {
            Vector2 currentVelocity = rb.linearVelocity;
            Vector2 gravityDirection = directionToPlanet;
            float radialVelocity = Vector2.Dot(currentVelocity, gravityDirection);
            Vector2 radialComponent = gravityDirection * radialVelocity;
            
            rb.linearVelocity = targetVelocity * 0.3f + radialComponent;
        }
    }
    
    public void Jump()
    {
        if (planet == null || !isGrounded) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 jumpDirection = -directionToPlanet;
        
        rb.AddForce(jumpDirection * (jumpForce * jumpMultiplier), ForceMode2D.Impulse);
    }
    
    public void FlipSprite(float moveDirection)
    {
        bool shouldFaceRight = moveDirection > 0;
        
        if (shouldFaceRight != facingRight)
        {
            facingRight = shouldFaceRight;
            
            Vector3 currentScale = transform.localScale;
            currentScale.x = facingRight ? Mathf.Abs(currentScale.x) : -Mathf.Abs(currentScale.x);
            transform.localScale = currentScale;
        }
    }
    
    void OnDrawGizmos()
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, directionToPlanet * groundCheckDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        
        if (planet.center != null)
        {
            Gizmos.color = player != null && player.playerNumber == 1 ? Color.blue : Color.red;
            Vector2 center = planet.center.position;
            float radius = planet.radius + planet.playerOffset;
            
            DrawWireCircle(center, radius);
        }
    }
    
    void DrawWireCircle(Vector2 center, float radius)
    {
        int segments = 32;
        float angleStep = 2f * Mathf.PI / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep;
            float angle2 = (i + 1) * angleStep;
            
            Vector2 point1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * radius;
            Vector2 point2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * radius;
            
            Gizmos.DrawLine(point1, point2);
        }
    }
}
using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public float moveSpeed = 3f;
    public float kickForce = 10f;
    public float kickRange = 1.5f;
    public float jumpForce = 8f;
    public float gravityStrength = 50f;
    
    [Header("Ground Check")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 1.0f;
    public float groundCheckRadius = 0.3f;
    
    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    public KeyCode kickKey = KeyCode.S;
    
    private Planet planet;
    private float currentAngle;
    private Rigidbody2D rb;
    private bool isGrounded;
    private Vector2 lastGroundNormal;
    private SpriteRenderer spriteRenderer;
    private bool facingRight = true;
    
    void Start()
    {
        planet = FindObjectOfType<Planet>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        SetupPlayerKeys();
        
        if (planet != null)
        {
            currentAngle = planet.GetAngleFromPosition(transform.position);
        }
    }
    
    void SetupPlayerKeys()
    {
        if (playerNumber == 1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;
            kickKey = KeyCode.S;
        }
        else if (playerNumber == 2)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
            kickKey = KeyCode.DownArrow;
        }
    }
    
    void Update()
    {
        CheckGrounded();
        HandleInput();
    }
    
    void FixedUpdate()
    {
        if (planet != null)
        {
            ApplyPlanetGravity();
            OrientToPlanet();
        }
    }
    
    void CheckGrounded()
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        
        float distanceToCenter = Vector2.Distance(transform.position, planet.center.position);
        float surfaceDistance = distanceToCenter - planet.radius;
        
        bool wasGrounded = isGrounded;
        
        isGrounded = surfaceDistance <= groundCheckDistance;
        
        if (isGrounded)
        {
            lastGroundNormal = -directionToPlanet;
        }
        
        if (!wasGrounded && isGrounded)
        {
            rb.linearVelocity *= 0.8f;
        }
        
        Debug.Log($"Player {playerNumber} - Distance to surface: {surfaceDistance:F2}, Grounded: {isGrounded}");
    }
    
    void HandleInput()
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
        
        if (playerNumber == 2)
            moveInput = -moveInput;
        
        if (Mathf.Abs(moveInput) > 0.1f && isGrounded)
        {
            MoveAroundPlanet(moveInput);
            FlipSprite(moveInput);
        }
        
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                Debug.Log($"Player {playerNumber} tried to jump in air - blocked!");
            }
        }
        
        if (Input.GetKeyDown(kickKey))
        {
            TryKickBall();
        }
    }
    
    void MoveAroundPlanet(float direction)
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 tangentDirection;
        
        if (playerNumber == 1)
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
    
    void ApplyPlanetGravity()
    {
        Vector2 directionToPlanet = (Vector2)planet.center.position - (Vector2)transform.position;
        float distanceToCenter = directionToPlanet.magnitude;
        
        Vector2 gravityForce = directionToPlanet.normalized * gravityStrength;
        rb.AddForce(gravityForce);
        
        if (Time.fixedTime % 1f < Time.fixedDeltaTime)
        {
            Debug.Log($"Player {playerNumber} gravity: {gravityForce}, distance: {distanceToCenter}");
        }
    }
    
    void Jump()
    {
        if (planet == null || !isGrounded) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 jumpDirection = -directionToPlanet;
        
        Debug.Log($"Player {playerNumber} jumping. Direction: {jumpDirection}, Force: {jumpForce}");
        rb.AddForce(jumpDirection * jumpForce, ForceMode2D.Impulse);
    }
    
    void OrientToPlanet()
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        
        float angle = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg - 90f;
        
        Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        
        if (isGrounded)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 10f);
        }
        else
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3f);
        }
    }
    
    void TryKickBall()
    {
        Ball ballComponent = FindObjectOfType<Ball>();
        if (ballComponent == null) return;
        
        GameObject ball = ballComponent.gameObject;
        float distanceToBall = Vector2.Distance(transform.position, ball.transform.position);
        
        if (distanceToBall <= kickRange)
        {
            KickBall(ball);
        }
    }
    
    void KickBall(GameObject ball)
    {
        Rigidbody2D ballRb = ball.GetComponent<Rigidbody2D>();
        if (ballRb == null || planet == null) return;
        
        Vector2 playerToBall = (ball.transform.position - transform.position).normalized;
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        
        Vector2 playerSurfaceNormal = -directionToPlanet;
        
        Vector2 kickDirection = Vector2.Reflect(playerToBall, playerSurfaceNormal);
        
        Vector2 playerMovement = rb.linearVelocity;
        Vector2 tangentDirection = Vector2.Perpendicular(directionToPlanet);
        
        if (Vector2.Dot(playerMovement, tangentDirection) < 0)
            tangentDirection = -tangentDirection;
        
        Vector2 finalKickDirection = (kickDirection + tangentDirection * 0.3f).normalized;
        
        float dynamicKickForce = kickForce + rb.linearVelocity.magnitude * 2f;
        
        ballRb.AddForce(finalKickDirection * dynamicKickForce, ForceMode2D.Impulse);
        
        Debug.Log($"Player {playerNumber} kicked the ball! Direction: {finalKickDirection}, Force: {dynamicKickForce}");
    }
    
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        DrawWireCircle(transform.position, kickRange);
    }
    
    void DrawWireCircle(Vector3 center, float radius)
    {
        int segments = 32;
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
    
    void FlipSprite(float moveDirection)
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
    }
}
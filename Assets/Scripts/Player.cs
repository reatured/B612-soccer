using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public float moveSpeed = 3f;
    public float collisionKickForce = 15f;
    public float jumpForce = 8f;
    public float gravityStrength = 50f;
    
    [Header("Ground Check")]
    public LayerMask groundLayerMask = 1;
    public float groundCheckDistance = 1.0f;
    public float groundCheckRadius = 0.3f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip kickBallSound;
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip moveSound;
    
    [Header("Power-ups")]
    public float powerKickMultiplier = 1f;
    public float sizeMultiplier = 1f;
    public float jumpMultiplier = 1f;
    public float powerUpDuration = 5f;
    
    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.W;
    
    private Planet planet;
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
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        SetupPlayerKeys();
    }
    
    void SetupPlayerKeys()
    {
        if (playerNumber == 1)
        {
            leftKey = KeyCode.A;
            rightKey = KeyCode.D;
            jumpKey = KeyCode.W;
        }
        else if (playerNumber == 2)
        {
            leftKey = KeyCode.LeftArrow;
            rightKey = KeyCode.RightArrow;
            jumpKey = KeyCode.UpArrow;
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
            EnforcePlayerBoundaries();
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
        
    }
    
    void MoveAroundPlanet(float direction)
    {
        if (planet == null) return;
        
        if (!CanMoveInDirection(direction))
        {
            return;
        }
        
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
    
    bool CanMoveInDirection(float direction)
    {
        if (planet == null) return true;
        
        Vector2 playerPosition = transform.position;
        Vector2 planetCenter = planet.center.position;
        
        float currentAngle = Mathf.Atan2(playerPosition.y - planetCenter.y, playerPosition.x - planetCenter.x);
        
        if (playerNumber == 1)
        {
            if (direction > 0 && currentAngle < 0.5f && currentAngle > -0.5f)
            {
                return false;
            }
        }
        else if (playerNumber == 2)
        {
            if (direction < 0 && (currentAngle > 2.64f || currentAngle < -2.64f))
            {
                return false;
            }
        }
        
        return true;
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
        rb.AddForce(jumpDirection * jumpForce * jumpMultiplier, ForceMode2D.Impulse);
        PlaySound(jumpSound);
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
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null && collision.contacts.Length > 0)
        {
            KickBallOnCollision(collision, ball);
        }
    }
    
    void KickBallOnCollision(Collision2D collision, Ball ball)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 collisionNormal = contact.normal;
        
        Vector2 kickDirection = -collisionNormal;
        
        Vector2 playerVelocity = rb.linearVelocity;
        Vector2 relativeVelocity = playerVelocity - ball.GetComponent<Rigidbody2D>().linearVelocity;
        
        float velocityMagnitude = relativeVelocity.magnitude;
        float kickStrength = (collisionKickForce + velocityMagnitude * 1.5f) * powerKickMultiplier;
        
        Vector2 tangentDirection = Vector2.Perpendicular(collisionNormal);
        if (Vector2.Dot(playerVelocity, tangentDirection) < 0)
            tangentDirection = -tangentDirection;
        
        Vector2 finalKickDirection = (kickDirection + tangentDirection * 0.4f).normalized;
        
        ball.GetComponent<Rigidbody2D>().AddForce(finalKickDirection * kickStrength, ForceMode2D.Impulse);
        ball.ActivateTrail();
        PlaySound(kickBallSound);
        
        Debug.Log($"Player {playerNumber} collided with ball! Normal: {collisionNormal}, Kick Direction: {finalKickDirection}, Force: {kickStrength}");
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
    
    void EnforcePlayerBoundaries()
    {
        if (planet == null) return;
        
        Vector2 playerPosition = transform.position;
        Vector2 planetCenter = planet.center.position;
        
        float currentAngle = Mathf.Atan2(playerPosition.y - planetCenter.y, playerPosition.x - planetCenter.x);
        
        bool needsCorrection = false;
        float targetAngle = currentAngle;
        
        if (playerNumber == 1)
        {
            if (currentAngle < 0.5f && currentAngle > -0.5f)
            {
                targetAngle = currentAngle > 0 ? 0.5f : -0.5f;
                needsCorrection = true;
            }
        }
        else if (playerNumber == 2)
        {
            if (currentAngle > 2.64f || currentAngle < -2.64f)
            {
                targetAngle = currentAngle > 0 ? 2.64f : -2.64f;
                needsCorrection = true;
            }
        }
        
        if (needsCorrection)
        {
            float distanceFromCenter = Vector2.Distance(playerPosition, planetCenter);
            Vector2 correctedPosition = planetCenter + new Vector2(Mathf.Cos(targetAngle), Mathf.Sin(targetAngle)) * distanceFromCenter;
            transform.position = correctedPosition;
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
            Gizmos.color = playerNumber == 1 ? Color.blue : Color.red;
            Vector2 center = planet.center.position;
            float radius = planet.radius + planet.playerOffset;
            
            if (playerNumber == 1)
            {
                Vector3 start1 = center + new Vector2(Mathf.Cos(-0.5f), Mathf.Sin(-0.5f)) * radius;
                Vector3 start2 = center + new Vector2(Mathf.Cos(0.5f), Mathf.Sin(0.5f)) * radius;
                Gizmos.DrawLine(center, start1);
                Gizmos.DrawLine(center, start2);
            }
            else
            {
                Vector3 start1 = center + new Vector2(Mathf.Cos(-2.64f), Mathf.Sin(-2.64f)) * radius;
                Vector3 start2 = center + new Vector2(Mathf.Cos(2.64f), Mathf.Sin(2.64f)) * radius;
                Gizmos.DrawLine(center, start1);
                Gizmos.DrawLine(center, start2);
            }
        }
    }
    
    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
    
    public void ApplyPowerUp(PowerUpType powerUpType)
    {
        StopCoroutine(nameof(PowerUpCoroutine));
        StartCoroutine(PowerUpCoroutine(powerUpType));
    }
    
    System.Collections.IEnumerator PowerUpCoroutine(PowerUpType powerUpType)
    {
        switch (powerUpType)
        {
            case PowerUpType.PowerfulKick:
                powerKickMultiplier = 2f;
                break;
            case PowerUpType.BiggerBody:
                sizeMultiplier = 1.5f;
                transform.localScale = Vector3.one * sizeMultiplier;
                break;
            case PowerUpType.HigherJump:
                jumpMultiplier = 2f;
                break;
        }
        
        yield return new WaitForSeconds(powerUpDuration);
        
        powerKickMultiplier = 1f;
        sizeMultiplier = 1f;
        jumpMultiplier = 1f;
        transform.localScale = Vector3.one;
    }
}

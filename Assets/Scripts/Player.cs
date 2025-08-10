using UnityEngine;

public class Player : MonoBehaviour
{
    [Header("Player Settings")]
    public int playerNumber = 1;
    public float moveSpeed = 3f;
    public float collisionKickForce = 15f;
    public float jumpForce = 8f;
    public float gravityStrength = 50f;
    public float gravityFadeDistance = 10f;
    
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
    private bool facingRight = true;
    
    [Header("Footstep Audio")]
    private bool isMoving = false;
    private bool isPlayingFootsteps = false;
    private Coroutine footstepCoroutine;
    
    void Start()
    {
        planet = FindFirstObjectByType<Planet>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        
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
        }
    }
    
    void CheckGrounded()
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
        
        // Debug.Log($"Player {playerNumber} - Distance to surface: {surfaceDistance:F2}, Grounded: {isGrounded}");
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
        

        
        bool wasMoving = isMoving;
        isMoving = Mathf.Abs(moveInput) > 0.1f && isGrounded;
        
        if (isMoving)
        {
            MoveAroundPlanet(moveInput);
            FlipSprite(moveInput);
            
            // Start footstep sounds if not already playing
            if (!isPlayingFootsteps)
            {
                StartFootstepSounds();
            }
        }
        else if (wasMoving && !isMoving)
        {
            // Player stopped moving, schedule footstep stop after 0.3 seconds
            if (footstepCoroutine != null)
            {
                StopCoroutine(footstepCoroutine);
            }
            footstepCoroutine = StartCoroutine(StopFootstepsAfterDelay());
        }
        
        if (Input.GetKeyDown(jumpKey))
        {
            if (isGrounded)
            {
                Jump();
            }
            else
            {
                // Debug.Log($"Player {playerNumber} tried to jump in air - blocked!");
            }
        }
        
    }
    
    void MoveAroundPlanet(float direction)
    {
        if (planet == null) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 tangentDirection;
        

        tangentDirection = new Vector2(-directionToPlanet.y, directionToPlanet.x) * direction;
        
        
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
        float surfaceDistance = distanceToCenter - planet.radius;
        
        if (surfaceDistance > gravityFadeDistance)
        {
            return;
        }
        
        Vector2 gravityForce = directionToPlanet.normalized * gravityStrength;
        
        float distanceFactor = 1f - Mathf.Clamp01(surfaceDistance / gravityFadeDistance);
        gravityForce *= distanceFactor;
        
        rb.AddForce(gravityForce);
        
        if (Time.fixedTime % 1f < Time.fixedDeltaTime)
        {
            // Debug.Log($"Player {playerNumber} gravity: {gravityForce}, distance: {distanceToCenter}, factor: {distanceFactor}");
        }
    }
    
    void Jump()
    {
        if (planet == null || !isGrounded) return;
        
        Vector2 directionToPlanet = ((Vector2)planet.center.position - (Vector2)transform.position).normalized;
        Vector2 jumpDirection = -directionToPlanet;
        
        //Debug.Log($"Player {playerNumber} jumping. Direction: {jumpDirection}, Force: {jumpForce}");
        rb.AddForce(jumpDirection * (jumpForce * jumpMultiplier), ForceMode2D.Impulse);
        
        // Use AudioManager for jump sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayJumpSound();
        }
        else
        {
            // Fallback to local audio clip
            PlaySound(jumpSound);
        }
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
        if (collision.contacts.Length > 0 && ball != null)
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
        
        // Play kick visual effects
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayKickEffect(contact.point, finalKickDirection);
        }
        
        // Use AudioManager for kick sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayKickSound();
        }        
        // Debug.Log($"Player {playerNumber} collided with ball! Normal: {collisionNormal}, Kick Direction: {finalKickDirection}, Force: {kickStrength}");
    }
    
    
    
    
    void FlipSprite(float moveDirection)
    {
        bool shouldFaceRight = moveDirection < 0;
        
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
        
        // Ground check visualization
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawRay(transform.position, directionToPlanet * groundCheckDistance);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        
        // Show full circular movement range (no restrictions)
        if (planet.center != null)
        {
            Gizmos.color = playerNumber == 1 ? Color.blue : Color.red;
            Vector2 center = planet.center.position;
            float radius = planet.radius + planet.playerOffset;
            
            // Draw a full circle to show unrestricted movement
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
    
    void StartFootstepSounds()
    {
        // Only start footsteps if not already playing
        if (isPlayingFootsteps) return;
        
        isPlayingFootsteps = true;
        
        // Stop any existing footstep coroutine
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        
        // Play one footstep sound immediately - only one per movement session
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayFootstepSound();
        }
    }
    
    System.Collections.IEnumerator StopFootstepsAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        isPlayingFootsteps = false;
        footstepCoroutine = null;
    }
    
    void PlaySound(AudioClip clip)
    {
        // Use AudioManager if available, otherwise use local audio source
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip);
        }
        else if (audioSource != null && clip != null)
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

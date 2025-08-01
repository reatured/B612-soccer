using UnityEngine;

public class Ball : MonoBehaviour
{
    [Header("Ball Physics")]
    public float bounciness = 0.8f;
    public float drag = 0.5f;
    public float angularDrag = 0.3f;
    public float maxSpeed = 15f;
    
    [Header("Planet Physics")]
    public float gravityStrength = 30f;
    public bool useOrbitPhysics = true;
    
    [Header("Trail Effect")]
    public TrailRenderer trail;
    public float trailTime = 0.5f;
    
    private Planet planet;
    private Rigidbody2D rb;
    private Vector2 lastVelocity;
    
    void Start()
    {
        planet = FindObjectOfType<Planet>();
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        SetupPhysics();
        SetupTrail();
    }
    
    void SetupPhysics()
    {
        rb.gravityScale = 0f;
        rb.linearDamping = drag;
        rb.angularDamping = angularDrag;
        
        PhysicsMaterial2D bouncyMaterial = new PhysicsMaterial2D("BallMaterial");
        bouncyMaterial.bounciness = bounciness;
        bouncyMaterial.friction = 0.4f;
        
        CircleCollider2D collider = GetComponent<CircleCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<CircleCollider2D>();
        }
        collider.sharedMaterial = bouncyMaterial;
    }
    
    void SetupTrail()
    {
        if (trail == null)
        {
            trail = GetComponent<TrailRenderer>();
        }
        
        if (trail != null)
        {
            trail.time = trailTime;
            trail.startWidth = 0.2f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
        }
    }
    
    void FixedUpdate()
    {
        if (planet != null && useOrbitPhysics)
        {
            ApplyPlanetGravity();
        }
        
        ClampVelocity();
        lastVelocity = rb.linearVelocity;
    }
    
    void ApplyPlanetGravity()
    {
        Vector2 directionToPlanet = (Vector2)planet.center.position - (Vector2)transform.position;
        float distanceToCenter = directionToPlanet.magnitude;
        
        if (distanceToCenter > planet.radius * 0.1f)
        {
            Vector2 gravityForce = directionToPlanet.normalized * gravityStrength;
            
            float distanceFactor = Mathf.Clamp01(distanceToCenter / (planet.radius * 2f));
            gravityForce *= distanceFactor;
            
            rb.AddForce(gravityForce);
        }
    }
    
    void ClampVelocity()
    {
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null)
        {
            OnPlayerHit(collision);
        }
        else if (collision.gameObject.GetComponent<Planet>() != null || collision.gameObject.name.Contains("Planet"))
        {
            OnPlanetBounce(collision);
        }
    }
    
    void OnPlayerHit(Collision2D collision)
    {
        Player player = collision.gameObject.GetComponent<Player>();
        if (player != null && collision.contacts.Length > 0)
        {
            ContactPoint2D contact = collision.contacts[0];
            Vector2 surfaceNormal = contact.normal;
            
            Vector2 kickDirection = -surfaceNormal;
            
            Vector2 playerVelocity = collision.rigidbody.linearVelocity;
            Vector2 tangentDirection = Vector2.Perpendicular(surfaceNormal);
            
            if (Vector2.Dot(playerVelocity, tangentDirection) < 0)
                tangentDirection = -tangentDirection;
            
            Vector2 finalKickDirection = (kickDirection + tangentDirection * 0.5f).normalized;
            
            float kickStrength = 8f + playerVelocity.magnitude * 0.5f;
            
            rb.AddForce(finalKickDirection * kickStrength, ForceMode2D.Impulse);
            
            Debug.Log($"Ball kicked by Player {player.playerNumber}. Direction: {finalKickDirection}, Strength: {kickStrength}");
        }
    }
    
    void OnPlanetBounce(Collision2D collision)
    {
        Vector2 incomingVector = lastVelocity;
        Vector2 reflectVector = Vector2.Reflect(incomingVector, collision.contacts[0].normal);
        
        rb.linearVelocity = reflectVector * bounciness;
        
        PlayBounceEffect();
    }
    
    void PlayBounceEffect()
    {
        if (trail != null)
        {
            trail.Clear();
        }
    }
    
    public void ResetBall()
    {
        if (planet != null)
        {
            transform.position = planet.center.position + Vector3.up * (planet.radius + 1f);
        }
        
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        
        if (trail != null)
        {
            trail.Clear();
        }
    }
    
    public void AddForce(Vector2 force)
    {
        rb.AddForce(force, ForceMode2D.Impulse);
    }
    
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.25f);
        
        if (rb != null && rb.linearVelocity.magnitude > 0.1f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
        }
    }
}
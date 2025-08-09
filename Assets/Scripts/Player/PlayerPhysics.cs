using UnityEngine;

public class PlayerPhysics : MonoBehaviour
{
    [Header("Physics Settings")]
    public float gravityStrength = 50f;
    public float gravityFadeDistance = 10f;
    public float collisionKickForce = 15f;
    
    private Planet planet;
    private Rigidbody2D rb;
    private Player player;
    private PlayerPowerUps powerUps;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GetComponent<Player>();
        powerUps = GetComponent<PlayerPowerUps>();
        planet = FindFirstObjectByType<Planet>();
        
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
    }
    
    public void ApplyPlanetGravity()
    {
        if (planet == null) return;
        
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
    }
    
    public void OrientToPlanet(bool isGrounded)
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
    
    public void HandleBallCollision(Collision2D collision, Ball ball)
    {
        ContactPoint2D contact = collision.contacts[0];
        Vector2 collisionNormal = contact.normal;
        
        Vector2 kickDirection = -collisionNormal;
        
        Vector2 playerVelocity = rb.linearVelocity;
        Vector2 relativeVelocity = playerVelocity - ball.GetComponent<Rigidbody2D>().linearVelocity;
        
        float velocityMagnitude = relativeVelocity.magnitude;
        float powerKickMultiplier = powerUps != null ? powerUps.powerKickMultiplier : 1f;
        float kickStrength = (collisionKickForce + velocityMagnitude * 1.5f) * powerKickMultiplier;
        
        Vector2 tangentDirection = Vector2.Perpendicular(collisionNormal);
        if (Vector2.Dot(playerVelocity, tangentDirection) < 0)
            tangentDirection = -tangentDirection;
        
        Vector2 finalKickDirection = (kickDirection + tangentDirection * 0.4f).normalized;
        
        ball.GetComponent<Rigidbody2D>().AddForce(finalKickDirection * kickStrength, ForceMode2D.Impulse);
        ball.ActivateTrail();
        
        if (VisualEffectsManager.Instance != null)
        {
            VisualEffectsManager.Instance.PlayKickEffect(contact.point, finalKickDirection);
        }
        
        return;
    }
    
    public Rigidbody2D GetRigidbody()
    {
        return rb;
    }
}
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Planet : MonoBehaviour
{
    [Header("Rotation")]
    public float baseRotationSpeed = 10f;
    public Vector3 rotationAxis = Vector3.up;
    
    [Header("Gravity")]
    public float gravityStrength = 9.81f;
    public float gravityRange = 50f;
    public LayerMask affectedLayers = -1;
    
    [Header("Speed Effects")]
    public float minSpeedMultiplier = 0.1f;
    public float maxSpeedMultiplier = 3f;
    public float speedTransitionRate = 2f;
    
    [Header("Debug")]
    public bool showGravityRange = true;
    
    private float currentSpeedMultiplier = 1f;
    private float targetSpeedMultiplier = 1f;
    private List<Rigidbody> affectedObjects = new List<Rigidbody>();
    
    private bool ShouldRotate 
    {
        get
        {
            if (GameManager.Instance == null) return false;
            return GameManager.Instance.GetCurrentState() == GameState.Game;
        }
    }
    
    void Start()
    {
        // Ensure we have a valid rotation axis
        if (rotationAxis == Vector3.zero)
            rotationAxis = Vector3.up;
    }

    void Update()
    {
        UpdateRotation();
        UpdateSpeedMultiplier();
    }

    void FixedUpdate()
    {
        ApplyGravity();
    }

    void UpdateRotation()
    {
        if (!ShouldRotate) return;
        
        float rotationSpeed = baseRotationSpeed * currentSpeedMultiplier;
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime, Space.Self);
    }

    void UpdateSpeedMultiplier()
    {
        if (Mathf.Abs(currentSpeedMultiplier - targetSpeedMultiplier) > 0.01f)
        {
            currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetSpeedMultiplier, 
                                              speedTransitionRate * Time.deltaTime);
        }
    }

    void ApplyGravity()
    {
        if (!ShouldRotate) return; // Only apply gravity during game
        
        // Find all objects in gravity range
        Collider[] colliders = Physics.OverlapSphere(transform.position, gravityRange, affectedLayers);
        
        // Clear the list and populate with current objects
        affectedObjects.Clear();
        
        foreach (Collider col in colliders)
        {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb != null && rb.gameObject != gameObject) // Don't affect self
            {
                affectedObjects.Add(rb);
            }
        }
        
        // Apply gravity to all found objects
        foreach (Rigidbody rb in affectedObjects)
        {
            ApplyGravityToObject(rb);
        }
    }

    void ApplyGravityToObject(Rigidbody rb)
    {
        Vector3 direction = (transform.position - rb.position).normalized;
        float distance = Vector3.Distance(transform.position, rb.position);
        
        // Prevent division by zero and extreme forces
        distance = Mathf.Max(distance, 1f);
        
        // Calculate gravitational force (F = G * m1 * m2 / r^2)
        // Simplified: F = gravityStrength * mass / distance^2
        float forceMagnitude = gravityStrength * rb.mass * currentSpeedMultiplier / (distance * distance);
        Vector3 force = direction * forceMagnitude;
        
        rb.AddForce(force, ForceMode.Force);
    }

    public void AddSpeedBoost(float amount, float duration = 0f)
    {
        if (duration > 0f)
        {
            StartCoroutine(TemporarySpeedEffect(amount, duration));
        }
        else
        {
            SetPermanentSpeedModifier(currentSpeedMultiplier + amount);
        }
    }

    public void AddSlowEffect(float amount, float duration = 0f)
    {
        if (duration > 0f)
        {
            StartCoroutine(TemporarySpeedEffect(-amount, duration));
        }
        else
        {
            SetPermanentSpeedModifier(currentSpeedMultiplier - amount);
        }
    }

    public void SetPermanentSpeedModifier(float multiplier)
    {
        targetSpeedMultiplier = Mathf.Clamp(multiplier, minSpeedMultiplier, maxSpeedMultiplier);
    }

    IEnumerator TemporarySpeedEffect(float speedChange, float duration)
    {
        float originalTarget = targetSpeedMultiplier;
        float newTarget = Mathf.Clamp(originalTarget + speedChange, minSpeedMultiplier, maxSpeedMultiplier);
        
        targetSpeedMultiplier = newTarget;
        
        yield return new WaitForSeconds(duration);
        
        targetSpeedMultiplier = originalTarget;
    }

    public float GetCurrentSpeedMultiplier()
    {
        return currentSpeedMultiplier;
    }

    public float GetCurrentRotationSpeed()
    {
        return baseRotationSpeed * currentSpeedMultiplier;
    }

    public float GetCurrentGravityStrength()
    {
        return gravityStrength * currentSpeedMultiplier;
    }

    void OnDrawGizmosSelected()
    {
        if (showGravityRange)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, gravityRange);
        }
    }
}
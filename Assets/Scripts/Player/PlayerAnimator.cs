using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    
    private PlayerMovement movement;
    private PlayerInput input;
    
    // Animation parameter names
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_GROUNDED = "IsGrounded";
    private const string PARAM_JUMP_TRIGGER = "Jump";
    private const string PARAM_KICK_TRIGGER = "Kick";
    private const string PARAM_MOVE_SPEED = "MoveSpeed";
    
    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
            
        movement = GetComponent<PlayerMovement>();
        input = GetComponent<PlayerInput>();
    }
    
    public void UpdateAnimationState()
    {
        if (animator == null) return;
        
        bool isMoving = input.IsMoving() && movement.isGrounded;
        animator.SetBool(PARAM_IS_MOVING, isMoving);
        animator.SetBool(PARAM_IS_GROUNDED, movement.isGrounded);
        animator.SetFloat(PARAM_MOVE_SPEED, Mathf.Abs(input.MoveInput));
    }
    
    public void TriggerJumpAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(PARAM_JUMP_TRIGGER);
    }
    
    public void TriggerKickAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger(PARAM_KICK_TRIGGER);
    }
    
    public void SetAnimationSpeed(float speed)
    {
        if (animator == null) return;
        animator.speed = speed;
    }
    
    public void PlayAnimation(string animationName)
    {
        if (animator == null) return;
        animator.Play(animationName);
    }
}
using UnityEngine;
using StateMachine;

public class PlayerJumpingState : PlayerBaseState
{
    private bool hasJumped = false;
    
    public override void OnEnter(PlayerStateMachine context)
    {
        LogStateChange("PlayerJumpingState");
        
        // Perform jump if we entered this state from a jump input
        if (context.input.JumpPressed && context.movement.isGrounded && !hasJumped)
        {
            PerformJump(context);
        }
        
        context.animator?.TriggerJumpAnimation();
        context.animator?.UpdateAnimationState();
    }
    
    public override void OnUpdate(PlayerStateMachine context)
    {
        // Allow air movement (limited)
        if (context.input.IsMoving())
        {
            context.movement.MoveAroundPlanet(context.input.MoveInput);
            context.movement.FlipSprite(context.input.MoveInput);
        }
        
        context.animator?.UpdateAnimationState();
    }
    
    public override void OnFixedUpdate(PlayerStateMachine context)
    {
        // Apply planet physics
        context.physics?.ApplyPlanetGravity();
        context.physics?.OrientToPlanet(context.movement.isGrounded);
        
        // Check if grounded
        context.movement?.CheckGrounded();
        
        // If we've landed, decide next state based on input
        if (context.movement.isGrounded)
        {
            // Play landing sound
            context.audio?.PlayLandSound();
            
            // Transition to appropriate state
            if (context.input.IsMoving())
            {
                context.ChangeState(new PlayerMovingState());
            }
            else
            {
                context.ChangeState(new PlayerIdleState());
            }
        }
    }
    
    public override void OnExit(PlayerStateMachine context)
    {
        hasJumped = false;
    }
    
    private void PerformJump(PlayerStateMachine context)
    {
        context.movement?.Jump();
        context.audio?.PlayJumpSound();
        hasJumped = true;
    }
    
    public override void OnCollisionEnter2D(PlayerStateMachine context, Collision2D collision)
    {
        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null && collision.contacts.Length > 0)
        {
            context.ChangeState(new PlayerKickingState(collision, ball));
        }
    }
}
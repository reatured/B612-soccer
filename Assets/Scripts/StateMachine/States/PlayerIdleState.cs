using UnityEngine;
using StateMachine;

public class PlayerIdleState : PlayerBaseState
{
    public override void OnEnter(PlayerStateMachine context)
    {
        LogStateChange("PlayerIdleState");
        
        context.animator?.UpdateAnimationState();
    }
    
    public override void OnUpdate(PlayerStateMachine context)
    {
        // Check for state transitions
        if (context.input.IsMoving() && context.movement.isGrounded)
        {
            context.ChangeState(new PlayerMovingState());
            return;
        }
        
        if (context.input.JumpPressed && context.movement.isGrounded)
        {
            context.ChangeState(new PlayerJumpingState());
            return;
        }
        
        context.animator?.UpdateAnimationState();
    }
    
    public override void OnFixedUpdate(PlayerStateMachine context)
    {
        // Apply planet physics
        context.physics?.ApplyPlanetGravity();
        context.physics?.OrientToPlanet(context.movement.isGrounded);
        
        // Check if still grounded
        context.movement?.CheckGrounded();
        
        // If not grounded, transition to jumping state
        if (!context.movement.isGrounded)
        {
            context.ChangeState(new PlayerJumpingState());
        }
    }
    
    public override void OnExit(PlayerStateMachine context)
    {
        // Nothing specific needed for idle state exit
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
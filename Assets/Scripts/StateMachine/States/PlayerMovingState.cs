using UnityEngine;
using StateMachine;

public class PlayerMovingState : PlayerBaseState
{
    private bool wasPlayingFootsteps = false;
    
    public override void OnEnter(PlayerStateMachine context)
    {
        LogStateChange("PlayerMovingState");
        
        context.animator?.UpdateAnimationState();
        
        // Start footstep audio
        if (!context.audio.IsPlayingFootsteps())
        {
            context.audio.StartMovementAudio();
            wasPlayingFootsteps = true;
        }
    }
    
    public override void OnUpdate(PlayerStateMachine context)
    {
        // Handle movement input
        if (context.input.IsMoving() && context.movement.isGrounded)
        {
            context.movement.MoveAroundPlanet(context.input.MoveInput);
            context.movement.FlipSprite(context.input.MoveInput);
        }
        else if (!context.input.IsMoving())
        {
            // Stopped moving, transition to idle
            context.ChangeState(new PlayerIdleState());
            return;
        }
        
        // Check for jump input
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
        // Stop footstep audio when exiting movement
        if (wasPlayingFootsteps)
        {
            context.audio.StopMovementAudio();
            wasPlayingFootsteps = false;
        }
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
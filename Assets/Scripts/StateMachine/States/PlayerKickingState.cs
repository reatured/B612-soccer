using UnityEngine;
using StateMachine;

public class PlayerKickingState : PlayerBaseState
{
    private Collision2D ballCollision;
    private Ball ball;
    private float kickDuration = 0.2f; // Brief kick state
    private float kickTimer = 0f;
    
    public PlayerKickingState(Collision2D collision, Ball ballComponent)
    {
        ballCollision = collision;
        ball = ballComponent;
    }
    
    public override void OnEnter(PlayerStateMachine context)
    {
        LogStateChange("PlayerKickingState");
        
        // Perform the kick
        PerformKick(context);
        
        // Trigger animation
        context.animator?.TriggerKickAnimation();
        
        kickTimer = 0f;
    }
    
    public override void OnUpdate(PlayerStateMachine context)
    {
        kickTimer += Time.deltaTime;
        
        // After kick duration, transition back to appropriate state
        if (kickTimer >= kickDuration)
        {
            TransitionToNextState(context);
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
    }
    
    public override void OnExit(PlayerStateMachine context)
    {
        ballCollision = null;
        ball = null;
    }
    
    private void PerformKick(PlayerStateMachine context)
    {
        if (ballCollision != null && ball != null)
        {
            // Handle ball collision through physics component
            context.physics?.HandleBallCollision(ballCollision, ball);
            
            // Play kick sound
            context.audio?.PlayKickSound();
        }
    }
    
    private void TransitionToNextState(PlayerStateMachine context)
    {
        // Check current conditions to decide next state
        if (!context.movement.isGrounded)
        {
            context.ChangeState(new PlayerJumpingState());
        }
        else if (context.input.IsMoving())
        {
            context.ChangeState(new PlayerMovingState());
        }
        else
        {
            context.ChangeState(new PlayerIdleState());
        }
    }
}
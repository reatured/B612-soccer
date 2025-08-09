using UnityEngine;
using StateMachine;

public class PlayerPoweredUpState : PlayerBaseState
{
    private PlayerBaseState wrappedState;
    private PowerUpType powerUpType;
    
    public PlayerPoweredUpState(PlayerBaseState stateToWrap, PowerUpType powerUp)
    {
        wrappedState = stateToWrap;
        powerUpType = powerUp;
    }
    
    public override void OnEnter(PlayerStateMachine context)
    {
        LogStateChange($"PlayerPoweredUpState({powerUpType})");
        
        // Apply the power-up
        context.powerUps?.ApplyPowerUp(powerUpType);
        
        // Enter the wrapped state
        wrappedState?.OnEnter(context);
    }
    
    public override void OnUpdate(PlayerStateMachine context)
    {
        // Check if power-up expired
        if (!context.powerUps.HasActivePowerUp())
        {
            // Power-up expired, transition to the wrapped state type without power-up
            TransitionToNormalState(context);
            return;
        }
        
        // Delegate to wrapped state
        wrappedState?.OnUpdate(context);
    }
    
    public override void OnFixedUpdate(PlayerStateMachine context)
    {
        // Delegate to wrapped state
        wrappedState?.OnFixedUpdate(context);
    }
    
    public override void OnExit(PlayerStateMachine context)
    {
        // Exit wrapped state
        wrappedState?.OnExit(context);
    }
    
    public override void OnCollisionEnter2D(PlayerStateMachine context, Collision2D collision)
    {
        // Handle ball collisions with enhanced power-up effects
        Ball ball = collision.gameObject.GetComponent<Ball>();
        if (ball != null && collision.contacts.Length > 0)
        {
            context.ChangeState(new PlayerKickingState(collision, ball));
        }
        else
        {
            // Delegate to wrapped state for other collisions
            wrappedState?.OnCollisionEnter2D(context, collision);
        }
    }
    
    private void TransitionToNormalState(PlayerStateMachine context)
    {
        // Transition to the appropriate normal state based on current conditions
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
    
    public PlayerBaseState GetWrappedState()
    {
        return wrappedState;
    }
    
    public PowerUpType GetPowerUpType()
    {
        return powerUpType;
    }
}
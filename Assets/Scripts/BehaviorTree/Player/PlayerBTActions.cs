using UnityEngine;
using BehaviorTree;
using StateMachine;

namespace BehaviorTree.Player
{
    public class MoveAction : BTAction
    {
        private PlayerStateMachine playerSM;
        private float direction;
        
        public MoveAction(PlayerStateMachine playerStateMachine, float moveDirection) : base("Move")
        {
            playerSM = playerStateMachine;
            direction = moveDirection;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null || !playerSM.movement.isGrounded)
                return BTNodeState.Failure;
            
            playerSM.movement.MoveAroundPlanet(direction);
            playerSM.movement.FlipSprite(direction);
            
            return BTNodeState.Success;
        }
    }
    
    public class JumpAction : BTAction
    {
        private PlayerStateMachine playerSM;
        
        public JumpAction(PlayerStateMachine playerStateMachine) : base("Jump")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null || !playerSM.movement.isGrounded)
                return BTNodeState.Failure;
            
            playerSM.movement.Jump();
            playerSM.audio.PlayJumpSound();
            
            return BTNodeState.Success;
        }
    }
    
    public class KickBallAction : BTAction
    {
        private PlayerStateMachine playerSM;
        private Ball targetBall;
        
        public KickBallAction(PlayerStateMachine playerStateMachine, Ball ball) : base("KickBall")
        {
            playerSM = playerStateMachine;
            targetBall = ball;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null || targetBall == null)
                return BTNodeState.Failure;
            
            // This would be triggered by collision detection in the actual game
            // Here we just check if we're close enough to the ball
            float distance = Vector2.Distance(playerSM.transform.position, targetBall.transform.position);
            
            if (distance <= 2f) // Kick range
            {
                playerSM.audio.PlayKickSound();
                return BTNodeState.Success;
            }
            
            return BTNodeState.Failure;
        }
    }
    
    public class ApplyPowerUpAction : BTAction
    {
        private PlayerStateMachine playerSM;
        private PowerUpType powerUpType;
        
        public ApplyPowerUpAction(PlayerStateMachine playerStateMachine, PowerUpType powerUp) : base("ApplyPowerUp")
        {
            playerSM = playerStateMachine;
            powerUpType = powerUp;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null || playerSM.powerUps.HasActivePowerUp())
                return BTNodeState.Failure;
            
            playerSM.powerUps.ApplyPowerUp(powerUpType);
            return BTNodeState.Success;
        }
    }
    
    public class IdleAction : BTAction
    {
        private PlayerStateMachine playerSM;
        
        public IdleAction(PlayerStateMachine playerStateMachine) : base("Idle")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            // Just update animation state for idle
            playerSM.animator?.UpdateAnimationState();
            
            return BTNodeState.Running; // Idle is a continuous state
        }
    }
}
using UnityEngine;
using BehaviorTree;
using StateMachine;

namespace BehaviorTree.Player
{
    public class IsGroundedCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public IsGroundedCondition(PlayerStateMachine playerStateMachine) : base("IsGrounded")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return playerSM.movement.isGrounded ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class IsMovingCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public IsMovingCondition(PlayerStateMachine playerStateMachine) : base("IsMoving")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return playerSM.input.IsMoving() ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class JumpPressedCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public JumpPressedCondition(PlayerStateMachine playerStateMachine) : base("JumpPressed")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return playerSM.input.JumpPressed ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class HasPowerUpCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public HasPowerUpCondition(PlayerStateMachine playerStateMachine) : base("HasPowerUp")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return playerSM.powerUps.HasActivePowerUp() ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class BallNearbyCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        private float detectionRadius;
        
        public BallNearbyCondition(PlayerStateMachine playerStateMachine, float radius = 5f) : base("BallNearby")
        {
            playerSM = playerStateMachine;
            detectionRadius = radius;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            // Find the ball in the scene
            Ball ball = Object.FindFirstObjectByType<Ball>();
            if (ball == null)
                return BTNodeState.Failure;
            
            float distance = Vector2.Distance(playerSM.transform.position, ball.transform.position);
            return distance <= detectionRadius ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class IsInAirCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public IsInAirCondition(PlayerStateMachine playerStateMachine) : base("IsInAir")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return !playerSM.movement.isGrounded ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class CanJumpCondition : BTCondition
    {
        private PlayerStateMachine playerSM;
        
        public CanJumpCondition(PlayerStateMachine playerStateMachine) : base("CanJump")
        {
            playerSM = playerStateMachine;
        }
        
        public override BTNodeState Evaluate()
        {
            if (playerSM == null)
                return BTNodeState.Failure;
            
            return (playerSM.movement.isGrounded && playerSM.input.JumpPressed) ? BTNodeState.Success : BTNodeState.Failure;
        }
    }
    
    public class PowerUpAvailableCondition : BTCondition
    {
        private PowerUpType powerUpType;
        
        public PowerUpAvailableCondition(PowerUpType powerUp) : base($"PowerUpAvailable({powerUp})")
        {
            powerUpType = powerUp;
        }
        
        public override BTNodeState Evaluate()
        {
            // This would check if a power-up of this type is available to collect
            // For now, we'll just return a placeholder
            return BTNodeState.Failure;
        }
    }
}
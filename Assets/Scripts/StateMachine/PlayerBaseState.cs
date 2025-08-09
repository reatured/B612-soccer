using UnityEngine;

namespace StateMachine
{
    public abstract class PlayerBaseState
    {
        public abstract void OnEnter(PlayerStateMachine context);
        public abstract void OnUpdate(PlayerStateMachine context);
        public abstract void OnFixedUpdate(PlayerStateMachine context);
        public abstract void OnExit(PlayerStateMachine context);
        
        public virtual void OnCollisionEnter2D(PlayerStateMachine context, Collision2D collision) { }
        public virtual void OnTriggerEnter2D(PlayerStateMachine context, Collider2D other) { }
        
        protected virtual void LogStateChange(string stateName)
        {
            Debug.Log($"Player entering state: {stateName}");
        }
    }
}
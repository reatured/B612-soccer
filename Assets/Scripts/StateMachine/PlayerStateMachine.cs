using UnityEngine;

namespace StateMachine
{
    public class PlayerStateMachine : MonoBehaviour
    {
        [Header("State Machine Debug")]
        public bool debugStateChanges = true;
        public string currentStateName = "None";
        
        private PlayerBaseState currentState;
        
        // References to player components
        public Player player { get; private set; }
        public PlayerMovement movement { get; private set; }
        public PlayerPhysics physics { get; private set; }
        public PlayerAudio audio { get; private set; }
        public PlayerPowerUps powerUps { get; private set; }
        public PlayerInput input { get; private set; }
        public PlayerAnimator animator { get; private set; }
        
        void Awake()
        {
            player = GetComponent<Player>();
            movement = GetComponent<PlayerMovement>();
            physics = GetComponent<PlayerPhysics>();
            audio = GetComponent<PlayerAudio>();
            powerUps = GetComponent<PlayerPowerUps>();
            input = GetComponent<PlayerInput>();
            animator = GetComponent<PlayerAnimator>();
        }
        
        void Update()
        {
            currentState?.OnUpdate(this);
        }
        
        void FixedUpdate()
        {
            currentState?.OnFixedUpdate(this);
        }
        
        public void ChangeState(PlayerBaseState newState)
        {
            if (currentState == newState) return;
            
            currentState?.OnExit(this);
            currentState = newState;
            currentStateName = currentState?.GetType().Name ?? "None";
            
            if (debugStateChanges)
            {
                Debug.Log($"Player {player.playerNumber} changed to state: {currentStateName}");
            }
            
            currentState?.OnEnter(this);
        }
        
        void OnCollisionEnter2D(Collision2D collision)
        {
            currentState?.OnCollisionEnter2D(this, collision);
        }
        
        void OnTriggerEnter2D(Collider2D other)
        {
            currentState?.OnTriggerEnter2D(this, other);
        }
        
        public PlayerBaseState GetCurrentState()
        {
            return currentState;
        }
        
        public T GetCurrentState<T>() where T : PlayerBaseState
        {
            return currentState as T;
        }
        
        public bool IsInState<T>() where T : PlayerBaseState
        {
            return currentState is T;
        }
    }
}
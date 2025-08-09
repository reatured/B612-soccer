using UnityEngine;

namespace BehaviorTree
{
    public enum BTNodeState
    {
        Running,
        Success,
        Failure
    }

    public abstract class BTNode
    {
        public BTNodeState state = BTNodeState.Running;
        public bool started = false;
        public string name;
        public BTNode parent;

        public BTNode()
        {
            name = this.GetType().Name;
        }

        public BTNode(string nodeName)
        {
            name = nodeName;
        }

        public abstract BTNodeState Evaluate();

        protected virtual void OnStart()
        {
        }

        protected virtual void OnStop()
        {
        }

        public BTNodeState Update()
        {
            if (!started)
            {
                OnStart();
                started = true;
            }

            state = Evaluate();

            if (state == BTNodeState.Failure || state == BTNodeState.Success)
            {
                OnStop();
                started = false;
            }

            return state;
        }

        public virtual void Abort()
        {
            OnStop();
            started = false;
            state = BTNodeState.Failure;
        }
    }
}
using UnityEngine;

namespace BehaviorTree
{
    public abstract class BTAction : BTNode
    {
        public BTAction() : base() { }
        public BTAction(string nodeName) : base(nodeName) { }

        protected override void OnStart()
        {
            Debug.Log($"Starting action: {name}");
        }

        protected override void OnStop()
        {
            Debug.Log($"Stopping action: {name}");
        }
    }
}
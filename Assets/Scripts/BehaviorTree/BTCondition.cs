using UnityEngine;

namespace BehaviorTree
{
    public abstract class BTCondition : BTNode
    {
        public BTCondition() : base() { }
        public BTCondition(string nodeName) : base(nodeName) { }

        protected override void OnStart()
        {
            Debug.Log($"Checking condition: {name}");
        }

        protected override void OnStop()
        {
            Debug.Log($"Condition finished: {name} - {state}");
        }
    }
}
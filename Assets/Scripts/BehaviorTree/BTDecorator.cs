using UnityEngine;

namespace BehaviorTree
{
    public abstract class BTDecorator : BTNode
    {
        protected BTNode child;

        public BTDecorator() : base() { }
        public BTDecorator(string nodeName) : base(nodeName) { }

        public BTDecorator SetChild(BTNode childNode)
        {
            child = childNode;
            child.parent = this;
            return this;
        }

        public override void Abort()
        {
            child?.Abort();
            base.Abort();
        }
    }

    public class BTInverter : BTDecorator
    {
        public BTInverter() : base() { }
        public BTInverter(string nodeName) : base(nodeName) { }

        public override BTNodeState Evaluate()
        {
            if (child == null)
                return BTNodeState.Failure;

            switch (child.Update())
            {
                case BTNodeState.Running:
                    return BTNodeState.Running;
                case BTNodeState.Success:
                    return BTNodeState.Failure;
                case BTNodeState.Failure:
                    return BTNodeState.Success;
            }

            return BTNodeState.Failure;
        }
    }

    public class BTRepeater : BTDecorator
    {
        private int maxRepeats = -1; // -1 means infinite
        private int currentRepeats = 0;

        public BTRepeater(int maxRepeats = -1) : base()
        {
            this.maxRepeats = maxRepeats;
        }

        public BTRepeater(string nodeName, int maxRepeats = -1) : base(nodeName)
        {
            this.maxRepeats = maxRepeats;
        }

        protected override void OnStart()
        {
            currentRepeats = 0;
        }

        public override BTNodeState Evaluate()
        {
            if (child == null)
                return BTNodeState.Failure;

            if (maxRepeats != -1 && currentRepeats >= maxRepeats)
                return BTNodeState.Success;

            switch (child.Update())
            {
                case BTNodeState.Running:
                    return BTNodeState.Running;
                case BTNodeState.Success:
                case BTNodeState.Failure:
                    currentRepeats++;
                    if (maxRepeats != -1 && currentRepeats >= maxRepeats)
                        return BTNodeState.Success;
                    child.started = false; // Reset child for next iteration
                    return BTNodeState.Running;
            }

            return BTNodeState.Running;
        }
    }

    public class BTCooldown : BTDecorator
    {
        private float cooldownTime;
        private float lastExecutionTime;

        public BTCooldown(float cooldownTime) : base()
        {
            this.cooldownTime = cooldownTime;
            lastExecutionTime = -cooldownTime;
        }

        public BTCooldown(string nodeName, float cooldownTime) : base(nodeName)
        {
            this.cooldownTime = cooldownTime;
            lastExecutionTime = -cooldownTime;
        }

        public override BTNodeState Evaluate()
        {
            if (child == null)
                return BTNodeState.Failure;

            if (Time.time - lastExecutionTime < cooldownTime)
                return BTNodeState.Failure;

            var result = child.Update();
            
            if (result == BTNodeState.Success || result == BTNodeState.Failure)
            {
                lastExecutionTime = Time.time;
            }

            return result;
        }
    }
}
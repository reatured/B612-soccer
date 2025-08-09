using System.Collections.Generic;
using UnityEngine;

namespace BehaviorTree
{
    public abstract class BTComposite : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public BTComposite() : base() { }
        public BTComposite(string nodeName) : base(nodeName) { }

        public BTComposite AddChild(BTNode child)
        {
            child.parent = this;
            children.Add(child);
            return this;
        }

        public virtual void RemoveChild(BTNode child)
        {
            children.Remove(child);
            child.parent = null;
        }

        public virtual void ClearChildren()
        {
            foreach (var child in children)
            {
                child.parent = null;
            }
            children.Clear();
        }

        public override void Abort()
        {
            foreach (var child in children)
            {
                child.Abort();
            }
            base.Abort();
        }
    }

    public class BTSelector : BTComposite
    {
        private int current = 0;

        public BTSelector() : base() { }
        public BTSelector(string nodeName) : base(nodeName) { }

        protected override void OnStart()
        {
            current = 0;
        }

        public override BTNodeState Evaluate()
        {
            for (int i = current; i < children.Count; ++i)
            {
                current = i;
                var child = children[current];

                switch (child.Update())
                {
                    case BTNodeState.Running:
                        return BTNodeState.Running;
                    case BTNodeState.Success:
                        return BTNodeState.Success;
                    case BTNodeState.Failure:
                        continue;
                }
            }

            return BTNodeState.Failure;
        }
    }

    public class BTSequence : BTComposite
    {
        private int current = 0;

        public BTSequence() : base() { }
        public BTSequence(string nodeName) : base(nodeName) { }

        protected override void OnStart()
        {
            current = 0;
        }

        public override BTNodeState Evaluate()
        {
            for (int i = current; i < children.Count; ++i)
            {
                current = i;
                var child = children[current];

                switch (child.Update())
                {
                    case BTNodeState.Running:
                        return BTNodeState.Running;
                    case BTNodeState.Failure:
                        return BTNodeState.Failure;
                    case BTNodeState.Success:
                        if (i == children.Count - 1)
                        {
                            return BTNodeState.Success;
                        }
                        continue;
                }
            }

            return BTNodeState.Success;
        }
    }

    public class BTParallel : BTComposite
    {
        public BTParallel() : base() { }
        public BTParallel(string nodeName) : base(nodeName) { }

        public override BTNodeState Evaluate()
        {
            bool anyChildRunning = false;
            bool anyChildFailed = false;

            foreach (var child in children)
            {
                switch (child.Update())
                {
                    case BTNodeState.Running:
                        anyChildRunning = true;
                        break;
                    case BTNodeState.Failure:
                        anyChildFailed = true;
                        break;
                    case BTNodeState.Success:
                        break;
                }
            }

            if (anyChildRunning)
                return BTNodeState.Running;
            
            if (anyChildFailed)
                return BTNodeState.Failure;
            
            return BTNodeState.Success;
        }
    }
}
using UnityEngine;

namespace BehaviorTree
{
    public class BehaviorTreeRunner : MonoBehaviour
    {
        private BTNode rootNode;
        public bool debugMode = true;

        public virtual BTNode SetupTree()
        {
            return null;
        }

        void Start()
        {
            rootNode = SetupTree();
        }

        protected virtual void Update()
        {
            if (rootNode != null)
            {
                rootNode.Update();
            }
        }

        public void SetRootNode(BTNode node)
        {
            rootNode = node;
        }

        public BTNode GetRootNode()
        {
            return rootNode;
        }
    }
}
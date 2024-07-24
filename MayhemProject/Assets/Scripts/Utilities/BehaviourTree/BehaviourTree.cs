using UnityEngine;

namespace BehaviourTree
{
    public abstract class BehaviourTree : MonoBehaviour
    {
        protected Node _root = null;

        protected virtual void Start()
        {
            _root = SetupTree();

            AttachRoot(_root);
        }

        protected virtual void Update()
        {
            if (_root != null)
                _root.Evaluate();
        }

        protected abstract Node SetupTree();

        private void AttachRoot(Node node)
        {
            node.Root = _root;

            if (node.Children.Count == 0)
                return;

            foreach (Node child in node.Children)
            {
                AttachRoot(child);
            }
        }

        private void OnDestroy()
        {
            DisposeNode(_root);
        }

        private void DisposeNode(Node node)
        {
            node.Dispose();

            if (node.Children.Count == 0)
                return;

            foreach (Node child in node.Children)
            {
                DisposeNode(child);
            }
        }
    }
}
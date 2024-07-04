using System;
using System.Collections.Generic;

//From video https://www.youtube.com/watch?v=aR6wt5BlE-E
namespace BehaviourTree
{
    public enum NodeState
    {
        RUNNING, SUCCESS, FAILURE
    }
    public abstract class Node : IDisposable
    {
        #region VARIABLES
        protected NodeState state;

        private Node _root;
        private Node _parent;

        protected List<Node> children = new List<Node>();

        private Dictionary<string, object> _dataContext = new Dictionary<string, object>();
        #endregion

        #region ACCESSORS
        public Node Root { get => _root; set => _root = value; }
        public Node Parent { get => _parent; set => _parent = value; }
        public List<Node> Children { get => children;}
        #endregion

        public Node()
        {
            _parent = null;
        }
        public Node(List<Node> children)
        {
            foreach (Node child in children)
            {
                Attach(child);
            }
        }

        private void Attach(Node node)
        {
            node.Parent = this;
            children.Add(node);
        }

        public abstract NodeState Evaluate();

        public void SetData(string key, object value)
        {
            _dataContext[key] = value;
        }

        public object GetData(string key)
        {
            //Do the node contains the data ?
            object value = null;
            if(_dataContext.TryGetValue(key, out value))
                return value;

            //Else we search upward in the tree (parent after parent)
            Node node = _parent;
            while (node != null)
            {
                value = node.GetData(key);
                if (value != null) 
                    return value;
                node = node.Parent;
            }
            return null;
        }

        public bool ClearData(string key)
        {
            //Do the node contains the data ?
            if (_dataContext.ContainsKey(key))
            {
                _dataContext.Remove(key);
                return true;
            }

            //Else we search upward in the tree (parent after parent)
            Node node = _parent;
            while (node != null)
            {
                bool cleared = node.ClearData(key);
                if (cleared)
                    return true;
                node = node.Parent;
            }
            return false;
        }

        public virtual void Dispose() {}
    }
}
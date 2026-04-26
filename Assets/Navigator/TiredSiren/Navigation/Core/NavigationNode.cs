using System.Collections.Generic;
using TiredSiren.Navigation.Controls;
using UnityEngine;

namespace TiredSiren.Navigation
{
    public class NavigationNode
    {
        public byte Depth { get; private set; }
        public NavigationNode Parent { get; private set; }
        public object Args { get; private set; }
        public UIControl Control { get; private set; }
        public IReadOnlyCollection<NavigationNode> Children => _children;
        
        private readonly Stack<NavigationNode> _children = new();

        public NavigationNode(object args, UIControl control)
        {
            Depth = 0;
            Args = args;
            Control = control;
        }
        
        public NavigationNode(object args, UIControl control, NavigationNode parent)
        {
            Depth = (byte)(parent.Depth + 1);
            Args = args;
            Parent = parent;
            Control = control;
            parent.AddChild(this);
        }
        
        public void AddChild(NavigationNode child)
        {
            if (child.Depth <= Depth)
            {
                Debug.LogError("Invalid navigation depth. Must be greater then parent one");
                return;
            }
            
            _children.Push(child);
        }

        public void RemoveLastChild()
        {
            _children.TryPop(out _);
        }
        
        public NavigationNode Find(byte depth, object args)
        {
            if (Depth == depth && Args == args)
                return this;

            foreach (var child in _children)
            {
                var result = child.Find(depth, args);
                if (result != null)
                    return result;
            }

            return null;
        }

        public NavigationNode LastChild() => _children.TryPeek(out var result) ? result : null;
    }
}
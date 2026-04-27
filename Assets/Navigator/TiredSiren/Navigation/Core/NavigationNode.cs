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
                LogError("Invalid navigation depth. Must be greater then parent one");
                return;
            }
            
            _children.Push(child);
        }

        public void RemoveLastChild()
        {
            _children.TryPop(out _);
        }
        
        /// <summary>
        /// Find specific node in tree
        /// </summary>
        /// <param name="depth">The target depth to search for.</param>
        /// <param name="args">The args reference to match against.</param>
        /// <returns></returns>
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
        
        /// <summary>
        /// Searches for a node with the specified depth and args by traversing only the top of each stack level.
        /// Returns null if the node is not found or if the current depth exceeds the target depth.
        /// </summary>
        /// <param name="depth">The target depth to search for.</param>
        /// <param name="args">The args reference to match against.</param>
        /// <returns>The matching <see cref="NavigationNode"/>, or null if not found.</returns>
        public NavigationNode FindLast(byte depth, object args)
        {
            if (Depth == depth && Args == args)
                return this;

            if (Depth >= depth)
                return null;

            return LastChild()?.Find(depth, args);
        }

        public NavigationNode LastChild() => _children.TryPeek(out var result) ? result : null;

        private void LogError(string message)
        {
            Debug.LogError($"[Navigation]: {message}");
        }
    }
}
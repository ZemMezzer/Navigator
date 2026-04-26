using System;
using TiredSiren.Navigation.Layout;
using UnityEngine;

namespace TiredSiren.Navigation.Resolvers
{
    public class NavigationDepthResolver : INavigationDepthResolver
    {
        private Transform[] _depthContainers = Array.Empty<Transform>();
        
        public Transform ResolveDepthContainer(int depth)
        {
            if (depth >= _depthContainers.Length)
            {
                Debug.LogError($"Depth container {depth} is out of bounds");
                return null;
            }
            
            var depthContainer = _depthContainers[depth];
            if (depthContainer == null)
                Debug.LogError($"Unable to resolve depth container: {depth}");
            
            return depthContainer;
        }
        
        public void InsertDepth(LayoutDepthContainer depthContainer)
        {
            if (depthContainer.Depth >= _depthContainers.Length)
                ResizeDepthContainers(depthContainer.Depth + 10);
            
            _depthContainers[depthContainer.Depth] = depthContainer.Container;
        }
        
        private void ResizeDepthContainers(int newSize)
        {
            var oldContainer = _depthContainers;
            _depthContainers = new Transform[newSize];

            for (var i = 0; i < oldContainer.Length; i++)
            {
                _depthContainers[i] = oldContainer[i];
            }
        }
    }
}
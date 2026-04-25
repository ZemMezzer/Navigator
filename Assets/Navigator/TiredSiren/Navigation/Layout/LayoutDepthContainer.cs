using UnityEngine;

namespace TiredSiren.Navigation.Layout
{
    public struct LayoutDepthContainer
    {
        public readonly byte Depth;
        public readonly Transform Container;

        public LayoutDepthContainer(byte depth, Transform container)
        {
            Depth = depth;
            Container = container;
        }
    }
}
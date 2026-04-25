using System;

namespace TiredSiren.Navigation.Arguments
{
    public record NavMetaData<T> where T : IUIModuleBehaviour
    {
        public readonly string LayoutName;
        public readonly byte Depth;
        public readonly Type ModuleType;

        public NavMetaData(string layoutName, byte depth)
        {
            LayoutName = layoutName;
            Depth = depth;
            ModuleType = typeof(T);
        }
    }
}
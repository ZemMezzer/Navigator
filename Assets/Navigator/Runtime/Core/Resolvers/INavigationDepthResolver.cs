using UnityEngine;

namespace TiredSiren.Navigation.Resolvers
{
    public interface INavigationDepthResolver
    {
        public Transform ResolveDepthContainer(int depth);
    }
}
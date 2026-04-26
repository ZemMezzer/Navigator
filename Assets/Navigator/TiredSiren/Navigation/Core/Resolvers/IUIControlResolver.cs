using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Controls;
using UnityEngine;

namespace TiredSiren.Navigation.Resolvers
{
    public interface IUIControlResolver
    {
        public UIControl Resolve<T>(Transform container, INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
    }
}
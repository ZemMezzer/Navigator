using R3;
using TiredSiren.Navigation.Arguments;

namespace TiredSiren.Navigation
{
    public interface INavigator
    {
        public ReadOnlyReactiveProperty<NavigationNode> Current { get; }
        public void Navigate<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
        public void CloseLast<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
    }
}
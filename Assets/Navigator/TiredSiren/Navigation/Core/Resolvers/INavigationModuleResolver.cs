using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Controls;

namespace TiredSiren.Navigation.Resolvers
{
    public interface INavigationModuleResolver
    {
        public IUIModuleBehaviour Resolve<T>(UIControl control, INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour;
    }
}
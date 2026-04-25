using R3;
using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Controls;
using VContainer;
using VContainer.Unity;

namespace TiredSiren.Navigation.Resolvers
{
    public class NavigationModuleResolver : INavigationModuleResolver
    {
        private readonly IObjectResolver _resolver;
        
        public NavigationModuleResolver(IObjectResolver resolver)
        {
            _resolver = resolver;
        }
        
        public IUIModuleBehaviour Resolve<T>(UIControl control, INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour
        {
            var moduleScope = _resolver.CreateScope(Installation);
            moduleScope.AddTo(control);
            return moduleScope.Resolve<T>();

            void Installation(IContainerBuilder builder)
            {
                builder.RegisterInstance(navigationArgs).AsSelf();
                builder.Register<T>(Lifetime.Singleton).AsSelf();
                EntryPointsBuilder.EnsureDispatcherRegistered(builder);
            }
        }
    }
}
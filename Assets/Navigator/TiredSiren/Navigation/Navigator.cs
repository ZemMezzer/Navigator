using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Resolvers;

namespace TiredSiren.Navigation
{
    public class Navigator : INavigator
    {
        private readonly INavigationDepthResolver _depthResolver;
        private readonly IUIControlResolver _controlResolver;
        private readonly INavigationModuleResolver _navigationModuleResolver;
        
        public Navigator(INavigationDepthResolver depthResolver, IUIControlResolver controlResolver, INavigationModuleResolver moduleResolver)
        {
            _depthResolver = depthResolver;
            _controlResolver = controlResolver;
            _navigationModuleResolver = moduleResolver;
        }

        public void Navigate<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour
        {
            var container = _depthResolver.ResolveDepthContainer(navigationArgs.NavMetaData.Depth);

            if (container == null)
                return;

            var control = _controlResolver.Resolve(container, navigationArgs);
            
            if(control == null)
                return;
            
            var module = _navigationModuleResolver.Resolve(control, navigationArgs);
            control.OnBehaviourUpdate(module);
        }
    }
}

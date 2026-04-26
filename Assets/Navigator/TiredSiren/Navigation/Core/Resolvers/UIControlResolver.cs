using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Controls;
using TiredSiren.Navigation.Layout;
using UnityEngine;

namespace TiredSiren.Navigation.Resolvers
{
    public class UIControlResolver : IUIControlResolver
    {
        private readonly ILayoutContainer _layoutContainer;

        public UIControlResolver(ILayoutContainer layoutContainer)
        {
            _layoutContainer = layoutContainer;
        }
        
        public UIControl Resolve<T>(Transform container, INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour
        {
            var layout = _layoutContainer.GetLayout(navigationArgs.NavMetaData.LayoutName);

            if (layout == null)
            {
                Debug.LogError($"Unable to resolve layout: {navigationArgs.NavMetaData.LayoutName}. Please make sure you are using the correct layout name and layout is presented in the layout container.");
                return null;
            }
            
            var instance = Object.Instantiate(layout, container);

            if (instance.TryGetComponent(out UIControl control)) 
                return control;
            
            Debug.LogError($"Unable to bind module to layout: {layout.name}, unable to find control");
            Object.Destroy(instance);
            return null;
        }
    }
}
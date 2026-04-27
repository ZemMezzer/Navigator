using System;
using R3;
using TiredSiren.Navigation.Arguments;
using TiredSiren.Navigation.Controls;
using TiredSiren.Navigation.Resolvers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TiredSiren.Navigation
{
    public class Navigator : INavigator, IDisposable
    {
        private readonly INavigationDepthResolver _depthResolver;
        private readonly IUIControlResolver _controlResolver;
        private readonly INavigationModuleResolver _navigationModuleResolver;

        private readonly NavigationNode _root;
        private NavigationNode _selected;
        
        private readonly ReactiveProperty<NavigationNode> _current;
        public ReadOnlyReactiveProperty<NavigationNode> Current => _current;
        
        public Navigator(INavigationDepthResolver depthResolver, IUIControlResolver controlResolver, INavigationModuleResolver moduleResolver)
        {
            _depthResolver = depthResolver;
            _controlResolver = controlResolver;
            _navigationModuleResolver = moduleResolver;
            
            _root = new NavigationNode(null, null);
            _current = new ReactiveProperty<NavigationNode>(_root);
            _selected = _root;
        }

        public void Navigate<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour
        {
            var container = _depthResolver.ResolveDepthContainer(navigationArgs.NavMetaData.Depth);

            if (container == null)
                return;

            var control = _controlResolver.Resolve(container, navigationArgs);
            
            if(control == null)
                return;
            
            var model = _navigationModuleResolver.Resolve(control, navigationArgs);
            control.SetModel(model);
            SyncNavigation(control, navigationArgs);
            _current.Value = _selected;
        }

        public void CloseLast<T>(INavigationArgs<T> navigationArgs) where T : IUIModuleBehaviour
        {
            var node = _root.FindLast(navigationArgs.NavMetaData.Depth, navigationArgs);

            if (node == null)
            {
                LogError($"Unable to find navigation node for '{navigationArgs.NavMetaData.LayoutName}' at depth {navigationArgs.NavMetaData.Depth}. Make sure the module has been opened before closing it.");
                return;
            }

            if (node.Parent == null)
            {
                LogError("Unable to close root module");
                return;
            }

            if (node.Parent.LastChild() != node)
            {
                LogError("Unable to close hidden module");
                return;
            }
            
            CloseInternal(node);
            _selected?.Control?.gameObject.SetActive(true);
            _current.Value = _selected;
        }

        private void CloseInternal(NavigationNode node)
        {
            while (node.LastChild() != null)
                CloseInternal(node.LastChild());

            node.Parent?.RemoveLastChild();

            if (_selected == node)
                _selected = node.Parent?.LastChild() ?? node.Parent;

            if (node.Control != null)
            {
                if(node.Control.Model is IDisposable disposable)
                    disposable.Dispose();
                
                Object.Destroy(node.Control.gameObject);
            }
        }

        private void SyncNavigation<T>(UIControl control, INavigationArgs<T> args) where T : IUIModuleBehaviour
        {
            var depth = args.NavMetaData.Depth;
            var parent = _selected;

            while (parent.Depth > depth && parent.Parent != null)
            {
                while (parent.LastChild() != null)
                    CloseInternal(parent.LastChild());

                CloseInternal(parent);
                parent = parent.Parent;
            }

            if (parent.Depth == depth)
            {
                parent.Control?.gameObject.SetActive(false);
                parent = parent.Parent;
            }
            else
            {
                parent.LastChild()?.Control?.gameObject.SetActive(false);
            }

            _selected = new NavigationNode(args, control, parent);
        }

        public void Dispose()
        {
            _current.Dispose();
        }
        
        private void LogError(string message)
        {
            Debug.LogError($"[Navigator]: {message}");
        }
    }
}

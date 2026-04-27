using System;
using System.Collections.Generic;
using UnityEngine;

namespace TiredSiren.Navigation.Layout
{
    [CreateAssetMenu(menuName = "TiredSiren/Navigation/Layouts Container")]
    public class LayoutsContainer : ScriptableObject, ILayoutContainer
    {
        [Serializable]
        public struct LayoutData
        {
            [SerializeField] private string name;
            [SerializeField] private GameObject prefab;
            
            public string Name => name;
            public GameObject Prefab => prefab;
        }
        
        [SerializeField] private List<LayoutData> layouts;
        
        [NonSerialized] private Dictionary<int, GameObject> _layouts;

        public GameObject GetLayout(string layoutName)
        {
            if (_layouts == null)
            {
                _layouts = new Dictionary<int, GameObject>();
                InitializeLayouts();
            }
            
            return _layouts.GetValueOrDefault(layoutName.GetHashCode());
        }

        private void InitializeLayouts()
        {
            foreach (var layout in layouts)
            {
                var hash = string.IsNullOrEmpty(layout.Name) ? layout.Prefab.name.GetHashCode() : layout.Name.GetHashCode();

                if (_layouts.TryGetValue(hash, out var collidingLayout))
                {
                    var currentLayout = string.IsNullOrEmpty(layout.Name) ? layout.Prefab.name : layout.Name;
                    LogError($"Layout {currentLayout} is already registered or colliding with: {collidingLayout.name}");
                    continue;
                }
                _layouts[hash] = layout.Prefab;
            }
        }

        private void LogError(string message)
        {
            Debug.LogError($"[Layouts Container]: {message}]");
        }
    }
}
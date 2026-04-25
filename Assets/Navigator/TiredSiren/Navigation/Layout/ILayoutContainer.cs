using JetBrains.Annotations;
using UnityEngine;

namespace TiredSiren.Navigation.Layout
{
    public interface ILayoutContainer
    {
        public GameObject GetLayout(string layoutName);
    }
}
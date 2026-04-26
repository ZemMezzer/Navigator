using UnityEngine;

namespace TiredSiren.Navigation.Controls
{
    public abstract class UIControl : MonoBehaviour
    {
        public IUIModuleBehaviour Model { get; private set; }
        public void SetModel(IUIModuleBehaviour behaviour)
        {
            Model = behaviour;
            OnModelUpdate(behaviour);
        }

        protected abstract void OnModelUpdate(IUIModuleBehaviour behaviour);
    }
}
using UnityEngine;

namespace TiredSiren.Navigation.Controls
{
    public abstract class UIControl : MonoBehaviour
    {
        public abstract void OnBehaviourUpdate(IUIModuleBehaviour behaviour);
    }
}
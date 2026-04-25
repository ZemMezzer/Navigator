namespace TiredSiren.Navigation.Arguments
{
    public interface INavigationArgs<T> where T : IUIModuleBehaviour
    {
        public NavMetaData<T> NavMetaData { get; }
    }
}
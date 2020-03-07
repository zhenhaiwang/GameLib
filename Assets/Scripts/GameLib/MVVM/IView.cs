namespace GameLib
{
    public interface IView<T> where T : BaseViewModel
    {
        T viewModel { get; }
        void Bind(T viewModel);
        void UnBind();
    }
}
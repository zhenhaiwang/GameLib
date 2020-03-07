using UnityEngine;

namespace GameLib
{
    public abstract class BaseViewMVVM<T> : MonoBehaviour, IView<T> where T : BaseViewModel, new()
    {
        [SerializeField]
        private bool m_AutoBindViewModel;

        private bool m_ViewModelBinded;

        public T viewModel { get; private set; }

        public void Bind(T viewModel)
        {
            if (Equals(this.viewModel, viewModel))
            {
                return;
            }

            this.viewModel = viewModel;

            if (!m_ViewModelBinded)
            {
                m_ViewModelBinded = true;
                OnListenViewModel();
            }

            this.viewModel.OnInit();
        }

        public void UnBind()
        {
            if (viewModel == null)
            {
                return;
            }

            viewModel.OnDestroy();
            viewModel = null;
        }

        private void Awake()
        {
            OnAwake();
        }

        private void Start()
        {
            if (m_AutoBindViewModel)
            {
                Bind(new T());
            }

            OnStart();
        }

        private void OnDestroy()
        {
            UnBind();
            OnDestroyed();
        }

        protected virtual void OnAwake() { }

        protected virtual void OnStart() { }

        protected virtual void OnDestroyed() { }

        protected virtual void OnListenViewModel() { }
    }
}
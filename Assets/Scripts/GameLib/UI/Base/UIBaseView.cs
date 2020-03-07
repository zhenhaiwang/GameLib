using UnityEngine;

namespace GameLib
{
    public abstract class UIBaseView : MonoBehaviour
    {
        protected bool m_WaitToUpdateView = true;

        protected void Awake()
        {
            OnAwake();
        }

        protected void Start()
        {
            OnStart();
        }

        protected void FixedUpdate()
        {
            if (m_WaitToUpdateView)
            {
                m_WaitToUpdateView = false;
                UpdateView();
                m_WaitToUpdateView = false;
            }

            OnFixedUpdate();
        }

        protected void Update()
        {
            if (m_WaitToUpdateView)
            {
                m_WaitToUpdateView = false;
                UpdateView();
                m_WaitToUpdateView = false;
            }

            OnUpdate();
        }

        protected void LateUpdate()
        {
            if (m_WaitToUpdateView)
            {
                m_WaitToUpdateView = false;
                UpdateView();
                m_WaitToUpdateView = false;
            }

            OnLateUpdate();
        }

        public void InvalidView()
        {
            m_WaitToUpdateView = true;
        }

        public virtual void UpdateView()
        {
            m_WaitToUpdateView = false;
        }

        protected virtual void OnAwake() { }
        protected virtual void OnStart() { }
        protected virtual void OnDestroy() { }
        protected virtual void OnFixedUpdate() { }
        protected virtual void OnUpdate() { }
        protected virtual void OnLateUpdate() { }
    }
}

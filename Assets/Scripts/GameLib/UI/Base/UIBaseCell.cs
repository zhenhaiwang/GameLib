using UnityEngine;

namespace GameLib
{
    public class UIBaseCell : UIBaseView
    {
        public int index { get; private set; }

        protected MonoBehaviour parentView { get; private set; }

        private object m_Data;

        public object data
        {
            get
            {
                return m_Data;
            }
            set
            {
                m_Data = value;

                if (m_Data == null)
                {
                    m_WaitToUpdateView = false;

                    OnRecycle();
                }
                else
                {
                    InvalidView();
                }
            }
        }

        protected virtual void OnRecycle() { }

        public void Provide(int index, object data, MonoBehaviour parentView = null)
        {
            this.index = index;
            this.data = data;

            this.parentView = parentView;
        }

        public void Recycle()
        {
            this.index = -1;
            this.data = null;

            parentView = null;
        }
    }
}
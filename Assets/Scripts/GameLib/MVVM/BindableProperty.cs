using System;

namespace GameLib
{
    public sealed class BindableProperty<T>
    {
        public Action<T> OnValueChanged;

        private T m_Value;

        public T Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (!Equals(m_Value, value))
                {
                    m_Value = value;
                    OnValueChanged.Call(m_Value);
                }
            }
        }

        public override string ToString()
        {
            return m_Value != null ? m_Value.ToString() : string.Empty;
        }
    }
}
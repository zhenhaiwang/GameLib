using UnityEngine;

namespace GameLib
{
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        protected static T m_Instance;

        public static T instance
        {
            get
            {
                if (m_Instance == null)
                {
                    m_Instance = SingletonManager.instance.GetSingleton<T>();
                }

                return m_Instance;
            }
        }

        protected virtual void OnDestroy()
        {
            m_Instance = null;
        }

        public void DoNothing() { }
    }
}
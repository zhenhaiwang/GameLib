namespace GameLib
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static readonly object m_Syslock = new object();

        private static T m_Instance;

        public static T instance
        {
            get
            {
                lock (m_Syslock)
                {
                    if (m_Instance == null)
                    {
                        new T();
                    }
                }

                return m_Instance;
            }
        }

        protected Singleton()
        {
            m_Instance = this as T;

            OnInit();
        }

        ~Singleton()
        {
            OnDestroy();
        }

        protected virtual void OnInit() { }

        protected virtual void OnDestroy() { }

        protected void DestroyInstance()
        {
            m_Instance = null;
        }

        public void DoNothing() { }
    }
}
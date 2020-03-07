using System.Threading;

namespace GameLib
{
    public abstract class BaseThread
    {
        private Thread m_Thread;
        private bool m_TerminateFlag;
        private object m_TerminateFlagMutex;

        public BaseThread()
        {
            m_Thread = new Thread(ThreadProcess);
            m_TerminateFlag = false;
            m_TerminateFlagMutex = new object();
        }

        public void Run()
        {
            m_Thread.Start(this);
        }

        protected static void ThreadProcess(object obj)
        {
            BaseThread me = (BaseThread)obj;
            me.Main();
        }

        protected virtual void Main() { }

        public void WaitTermination()
        {
            m_Thread.Join();
        }

        public void SetTerminated()
        {
            lock (m_TerminateFlagMutex)
            {
                m_TerminateFlag = true;
            }
        }

        public bool CheckTerminated()
        {
            lock (m_TerminateFlagMutex)
            {
                return m_TerminateFlag;
            }
        }

        public void Interrupt()
        {
            m_Thread.Interrupt();
        }
    }
}

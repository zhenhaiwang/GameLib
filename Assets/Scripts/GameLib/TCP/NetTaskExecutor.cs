using System;
using System.Collections.Generic;

namespace GameLib
{
    class NetTaskExecutor
    {
        private List<Action> m_Actions = new List<Action>();
        private List<Action> m_CurrentActions = new List<Action>();

        public void Update()
        {
            if (m_Actions.Count > 0)
            {
                lock (m_Actions)
                {
                    m_CurrentActions.Clear();
                    m_CurrentActions.AddRange(m_Actions);
                    m_Actions.Clear();
                }

                for (int i = 0; i < m_CurrentActions.Count; i++)
                {
                    m_CurrentActions[i].Call();
                }
            }
        }

        public void Add(Action action)
        {
            lock (m_Actions)
            {
                m_Actions.Add(action);
            }
        }

        public void Clear()
        {
            lock (m_Actions)
            {
                m_Actions.Clear();
            }
        }
    }
}

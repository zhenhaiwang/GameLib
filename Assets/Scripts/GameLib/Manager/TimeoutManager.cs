using System;
using System.Collections.Generic;

namespace GameLib
{
    public class TimeoutManager : MonoSingleton<TimeoutManager>
    {
        private Dictionary<long, TimeoutEntry> m_TimeoutEntryDict = new Dictionary<long, TimeoutEntry>();
        private HashSet<TimeoutEntry> m_TimeoutEntryHashs = new HashSet<TimeoutEntry>();

        private bool m_WaitCheck;

        public long CreateTimeout(float time, Action<long, object> callback, object param = null)
        {
            var entry = new TimeoutEntry(time, callback, param);

            m_TimeoutEntryDict[entry.id] = entry;
            m_WaitCheck = true;

            return entry.id;
        }

        public void ClearTimeout(long id, bool executeCallback = false)
        {
            if (!m_TimeoutEntryDict.ContainsKey(id))
            {
                return;
            }

            if (executeCallback)
            {
                m_TimeoutEntryDict[id].OnTimeout();
            }

            m_TimeoutEntryDict.Remove(id);
            m_WaitCheck = true;
        }

        public void ClearAllTimeout()
        {
            if (m_TimeoutEntryDict.Count > 0)
            {
                m_TimeoutEntryDict.Clear();
                m_WaitCheck = true;
            }
        }

        private void FixedUpdate()
        {
            if (m_WaitCheck)
            {
                CheckTimeout();
            }
        }

        private void CheckTimeout()
        {
            CancelInvoke("SetTimeout");

            m_WaitCheck = false;
            m_TimeoutEntryHashs.Clear();

            var now = DateTime.Now;

            float minTime = 0f;
            TimeoutEntry minEntry = null;

            foreach (var entry in m_TimeoutEntryDict.Values)
            {
                float deltaTime = (float)(now - entry.dateTime).TotalMilliseconds / 1000f;

                if (deltaTime >= entry.timeout)
                {
                    m_TimeoutEntryHashs.Add(entry);
                }
                else
                {
                    deltaTime = entry.timeout - deltaTime;

                    if (minEntry == null || deltaTime < minTime)
                    {
                        minTime = deltaTime;
                        minEntry = entry;
                    }
                }
            }

            foreach (var entry in m_TimeoutEntryHashs)
            {
                m_TimeoutEntryDict.Remove(entry.id);
                entry.OnTimeout();
            }

            if (minEntry != null)
            {
                Invoke("SetTimeout", minTime);
            }
        }

        private void SetTimeout()
        {
            m_WaitCheck = true;
        }

        private sealed class TimeoutEntry
        {
            private static long m_Sequence;

            public long id { get; private set; }
            public float timeout { get; private set; }
            public DateTime dateTime { get; private set; }

            private Action<long, object> m_TimeoutCallback;
            private object m_TimeoutParam;

            public TimeoutEntry(float time, Action<long, object> callback, object param = null)
            {
                id = ++m_Sequence;
                timeout = time;
                dateTime = DateTime.Now;

                m_TimeoutCallback = callback;
                m_TimeoutParam = param;
            }

            public void OnTimeout()
            {
                m_TimeoutCallback.Call(id, m_TimeoutParam);
                m_TimeoutCallback = null;
            }
        }
    }
}

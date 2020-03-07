using System;
using System.Collections;
using UnityEngine;

namespace GameLib
{
    /// <summary>
    /// Promise.Entry
    /// </summary>
    public sealed partial class Promise : MonoBehaviour
    {
        private sealed class Entry
        {
            public IEnumerator coroutine { get; private set; }

            private Action m_Action;
            private Action<object> m_ActionSingle;
            private Action<object, object> m_ActionDouble;

            private object m_ParameterFirst;
            private object m_ParameterSecond;

            public void Call()
            {
                if (coroutine != null)
                {
                    return;
                }

                if (m_Action != null)
                {
                    m_Action.Call();
                }
                else if (m_ActionSingle != null)
                {
                    m_ActionSingle.Call(m_ParameterFirst);
                }
                else if (m_ActionDouble != null)
                {
                    m_ActionDouble.Call(m_ParameterFirst, m_ParameterSecond);
                }
            }

            public void Release()
            {
                coroutine = null;

                m_Action = null;
                m_ActionSingle = null;
                m_ActionDouble = null;
                m_ParameterFirst = null;
                m_ParameterSecond = null;
            }

            public Entry Set(IEnumerator coroutine)
            {
                Release();

                this.coroutine = coroutine;

                return this;
            }

            public Entry Set(Action action)
            {
                Release();

                m_Action = action;

                return this;
            }

            public Entry Set(Action<object> actionSingle, object parameterFirst)
            {
                Release();

                m_ActionSingle = actionSingle;
                m_ParameterFirst = parameterFirst;

                return this;
            }

            public Entry Set(Action<object, object> actionDouble, object parameterFirst, object parameterSecond)
            {
                Release();

                m_ActionDouble = actionDouble;
                m_ParameterFirst = parameterFirst;
                m_ParameterSecond = parameterSecond;

                return this;
            }
        }
    }
}
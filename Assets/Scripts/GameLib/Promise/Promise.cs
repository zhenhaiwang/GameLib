using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameLib
{
    /// <summary>
    /// Promise.Main
    /// </summary>
    public sealed partial class Promise : MonoBehaviour
    {
        private readonly Queue<Entry> m_Entrys = new Queue<Entry>();

        public void Stop()
        {
            StopAllCoroutines();

            m_Entrys.Clear();
        }

        public Promise Then(IEnumerator coroutine)
        {
            if (coroutine != null)
            {
                Enqueue(GetEntry().Set(coroutine));
            }

            return this;
        }

        public Promise Then(Action action)
        {
            if (action != null)
            {
                Enqueue(GetEntry().Set(action));
            }

            return this;
        }

        public Promise Then(Action<object> action, object parameterFirst)
        {
            if (action != null)
            {
                Enqueue(GetEntry().Set(action, parameterFirst));
            }

            return this;
        }

        public Promise Then(Action<object, object> action, object parameterFirst, object parameterSecond)
        {
            if (action != null)
            {
                Enqueue(GetEntry().Set(action, parameterFirst, parameterSecond));
            }

            return this;
        }

        public Promise Wait(float time)
        {
            if (time > 0f)
            {
                Enqueue(GetEntry().Set(WaitForSeconds(time)));
            }

            return this;
        }

        public Promise WaitRealtime(float time)
        {
            if (time > 0f)
            {
                Enqueue(GetEntry().Set(WaitForSecondsRealtime(time)));
            }

            return this;
        }

        public Promise WaitOneFrame()
        {
            Enqueue(GetEntry().Set(WaitForEndOfFrame()));

            return this;
        }

        #region Private

        private IEnumerator WaitForSeconds(float time)
        {
            yield return new WaitForSeconds(time);
        }

        private IEnumerator WaitForSecondsRealtime(float time)
        {
            yield return new WaitForSecondsRealtime(time);
        }

        private IEnumerator WaitForEndOfFrame()
        {
            yield return new WaitForEndOfFrame();
        }

        private IEnumerator EntryCoroutine(Entry entry)
        {
            IEnumerator coroutine = entry.coroutine;

            while (coroutine.MoveNext())
            {
                yield return coroutine.Current;
            }

            FinishPromise();
        }

        private void StartPromise()
        {
            if (m_Entrys.Count == 0)
            {
                return;
            }

            var entry = m_Entrys.Peek();

            if (entry.coroutine != null)
            {
                StartCoroutine(EntryCoroutine(entry));
            }
            else
            {
                entry.Call();

                FinishPromise();
            }
        }

        private void FinishPromise()
        {
            if (m_Entrys.Count > 0)
            {
                Release(m_Entrys.Dequeue());

                StartPromise();
            }
        }

        private void Enqueue(Entry entry)
        {
            m_Entrys.Enqueue(entry);

            if (m_Entrys.Count == 1)
            {
                StartPromise();
            }
        }

        #endregion
    }
}

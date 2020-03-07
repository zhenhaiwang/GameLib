using UnityEngine;

namespace GameLib
{
    /// <summary>
    /// Promise.EntryPool
    /// </summary>
    public sealed partial class Promise : MonoBehaviour
    {
        private readonly ObjectPool<Entry> m_EntryPool = new ObjectPool<Entry>(null, entry => entry.Release());

        private const int ENTRY_POOL_SIZE = 512;    // max pooled size for promise-entry

        private Entry GetEntry()
        {
            return m_EntryPool.Get();
        }

        private void Release(Entry entry)
        {
            if (m_EntryPool.countInactive <= ENTRY_POOL_SIZE)
            {
                m_EntryPool.Release(entry);
            }
            else
            {
                entry.Release();
            }
        }
    }
}
using System.Collections.Generic;

namespace GameLib
{
    public static class ListPool<T>
    {
        private static readonly ObjectPool<List<T>> m_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());

        public static List<T> Get()
        {
            return m_ListPool.Get();
        }

        public static void Release(List<T> toRelease)
        {
            if (m_ListPool.countInactive < 5)
            {
                m_ListPool.Release(toRelease);
            }
            else
            {
                toRelease.Clear();
            }
        }
    }
}

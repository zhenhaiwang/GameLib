using UnityEngine;
using System.Collections.Generic;

namespace GameLib
{
    public class UISimpleObjectPool : UIObjectPool
    {
        // the prefab that this object pool returns instances of
        public GameObject prefab;
        // collection of currently inactive instances of the prefab
        private Stack<GameObject> m_InactiveInstances = new Stack<GameObject>();

        // returns an instance of the prefab
        public override GameObject GetObject(int index)
        {
            GameObject spawnedGameObject;

            // if there is an inactive instance of the prefab ready to return, return that
            if (m_InactiveInstances.Count > 0)
            {
                // remove the instance from teh collection of inactive instances
                spawnedGameObject = m_InactiveInstances.Pop();
            }
            // otherwise, create a new instance
            else
            {
                spawnedGameObject = Instantiate(prefab);

                var pooledObject = spawnedGameObject.GetComponent<UIPooledObject>();

                if (pooledObject == null)
                {
                    // add the PooledObject component to the prefab so we know it came from this pool
                    pooledObject = spawnedGameObject.AddComponent<UIPooledObject>();
                }

                pooledObject.pool = this;
            }

            // put the instance in the root of the scene and active it
            ActiveObject(spawnedGameObject);

            // return a reference to the instance
            return spawnedGameObject;
        }

        // return an instance of the prefab to the pool
        public override void ReturnObject(GameObject returnGameObject)
        {
            var pooledObject = returnGameObject.GetComponent<UIPooledObject>();

            // if the instance came from this pool, return it to the pool, and no maximum cache count exceeded
            if (pooledObject != null && pooledObject.pool == this && m_InactiveInstances.Count < m_MaxCount)
            {
                // make the instance a child of this and disable it
                DeactiveObject(returnGameObject);

                // add the instance to the collection of inactive instances
                m_InactiveInstances.Push(returnGameObject);
            }
            // otherwise, just destroy it
            else
            {
                DestroyImmediate(returnGameObject);
            }
        }

        // preload instances before actual use
        public override void WarmPool(int count, string parameter = null)
        {
            // just return if count is less than zero
            if (count <= 0)
            {
                return;
            }

            // warm count must be less than max count, minus inactive instances if not empty
            int warmCount = Mathf.Min(count, m_MaxCount - m_InactiveInstances.Count);

            if (warmCount <= 0)
            {
                return;
            }

            // warm prefab instances of the prefab, and return to the pool
            for (int i = 0; i < warmCount; i++)
            {
                var warmGameObject = Instantiate(prefab);

                var pooledObject = warmGameObject.GetComponent<UIPooledObject>();

                if (pooledObject == null)
                {
                    pooledObject = warmGameObject.AddComponent<UIPooledObject>();
                }

                pooledObject.pool = this;

                ReturnObject(warmGameObject);
            }
        }
    }
}

using System;
using UnityEngine;
using System.Collections.Generic;

namespace GameLib
{
    public class UIMultiObjectPool : UIObjectPool
    {
        [Serializable]
        public struct PrefabWrapper
        {
            public string sign;
            public GameObject prefab;
        }

        [SerializeField] private PrefabWrapper[] m_PrefabWrappers;
        private Dictionary<string, Stack<GameObject>> m_SignInactiveInstances = new Dictionary<string, Stack<GameObject>>();

        public Func<int, string> OnSignGetter { get; set; }

        public override GameObject GetObject(int index)
        {
            if (OnSignGetter == null)
            {
                Log.Error("Please provide sign getter function");

                return null;
            }

            string sign = OnSignGetter(index);

            if (string.IsNullOrEmpty(sign))
            {
                Log.Error("Sign fetcher return null or empty value");

                return null;
            }

            GameObject spawnedGameObject;
            Stack<GameObject> inactiveInstances;

            if (m_SignInactiveInstances.TryGetValue(sign, out inactiveInstances) && inactiveInstances.Count > 0)
            {
                spawnedGameObject = inactiveInstances.Pop();
            }
            else
            {
                var signPrefab = GetSignPrefab(sign);

                if (signPrefab == null)
                {
                    return null;
                }

                spawnedGameObject = Instantiate(signPrefab);

                var pooledObject = spawnedGameObject.GetComponent<UIMultiPooledObject>();

                if (pooledObject == null)
                {
                    pooledObject = spawnedGameObject.AddComponent<UIMultiPooledObject>();
                }

                pooledObject.pool = this;
                pooledObject.sign = sign;
            }

            ActiveObject(spawnedGameObject);

            return spawnedGameObject;
        }

        public override void ReturnObject(GameObject returnGameObject)
        {
            var pooledObject = returnGameObject.GetComponent<UIMultiPooledObject>();

            if (pooledObject != null && pooledObject.pool == this)
            {
                Stack<GameObject> inactiveInstances;

                if (!m_SignInactiveInstances.TryGetValue(pooledObject.sign, out inactiveInstances))
                {
                    inactiveInstances = new Stack<GameObject>();
                    m_SignInactiveInstances.Add(pooledObject.sign, inactiveInstances);
                }

                if (inactiveInstances.Count < m_MaxCount)
                {
                    DeactiveObject(returnGameObject);

                    inactiveInstances.Push(returnGameObject);
                }
                else
                {
                    DestroyImmediate(returnGameObject);
                }
            }
            else
            {
                DestroyImmediate(returnGameObject);
            }
        }

        public override void WarmPool(int count, string parameter)
        {
            if (count <= 0 || string.IsNullOrEmpty(parameter))
            {
                return;
            }

            Stack<GameObject> inactiveInstances;
            int pooledCount = 0;

            if (m_SignInactiveInstances.TryGetValue(parameter, out inactiveInstances))
            {
                pooledCount = inactiveInstances.Count;
            }

            int warmCount = Mathf.Min(count, m_MaxCount - pooledCount);

            if (warmCount <= 0)
            {
                return;
            }

            var signPrefab = GetSignPrefab(parameter);

            if (signPrefab == null)
            {
                return;
            }

            for (int i = 0; i < warmCount; i++)
            {
                var warmGameObject = Instantiate(signPrefab);

                var pooledObject = warmGameObject.GetComponent<UIMultiPooledObject>();

                if (pooledObject == null)
                {
                    pooledObject = warmGameObject.AddComponent<UIMultiPooledObject>();
                }

                pooledObject.pool = this;
                pooledObject.sign = parameter;

                ReturnObject(warmGameObject);
            }
        }

        private GameObject GetSignPrefab(string sign)
        {
            int prefabWrapperLength = m_PrefabWrappers.Length();

            for (int i = 0; i < prefabWrapperLength; i++)
            {
                if (m_PrefabWrappers[i].sign == sign)
                {
                    return m_PrefabWrappers[i].prefab;
                }
            }

            Log.Error("There is no prefab that has sign: " + sign);

            return null;
        }
    }

    public class UIMultiPooledObject : UIPooledObject
    {
        public string sign { get; set; }
    }
}

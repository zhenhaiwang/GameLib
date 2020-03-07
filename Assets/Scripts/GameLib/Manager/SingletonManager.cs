using UnityEngine;

namespace GameLib
{
    public sealed class SingletonManager : MonoBehaviour
    {
        public static SingletonManager instance { get; private set; }

        private void Awake()
        {
            instance = this;

            gameObject.name = "SingletonManager";

            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            instance = null;
        }

        public T GetSingleton<T>() where T : class
        {
            var singletonType = typeof(T);
            string singletonName = singletonType.Name;

            var singletonTransform = transform.Find(singletonName);

            if (singletonTransform == null)
            {
                singletonTransform = new GameObject(singletonName).transform;
                singletonTransform.SetParent(transform);
            }

            var singletonComponent = singletonTransform.GetComponent(singletonType);

            if (singletonComponent == null)
            {
                singletonComponent = singletonTransform.gameObject.AddComponent(singletonType);
            }

            return singletonComponent as T;
        }
    }
}
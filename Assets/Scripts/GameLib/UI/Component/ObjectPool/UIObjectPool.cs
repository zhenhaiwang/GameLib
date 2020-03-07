using UnityEngine;

namespace GameLib
{
    public abstract class UIObjectPool : MonoBehaviour
    {
        protected enum StrategyType
        {
            ChangeActive = 0,
            ChangeScale,
        }

        [SerializeField] protected StrategyType m_Strategy = StrategyType.ChangeActive;     // the strategy used to active or deactive gameobject, when get from or return to pool
        [SerializeField] protected int m_MaxCount = 50;                                     // the max cached object count, default is 50

        public abstract GameObject GetObject(int index);
        public abstract void ReturnObject(GameObject returnGameObject);

        public virtual void WarmPool(int count, string parameter = null) { }

        protected void ActiveObject(GameObject activeGameObject)
        {
            if (activeGameObject == null)
            {
                return;
            }

            switch (m_Strategy)
            {
                case StrategyType.ChangeActive:
                    {
                        activeGameObject.SetActive(true);
                    }
                    break;
                case StrategyType.ChangeScale:
                    {
                        if (!activeGameObject.activeSelf)
                        {
                            activeGameObject.SetActive(true);
                        }

                        activeGameObject.transform.localScale = Vector3.one;
                    }
                    break;
            }
        }

        protected void DeactiveObject(GameObject deactiveGameObject)
        {
            if (deactiveGameObject == null)
            {
                return;
            }

            switch (m_Strategy)
            {
                case StrategyType.ChangeActive:
                    {
                        deactiveGameObject.SetActive(false);
                    }
                    break;
                case StrategyType.ChangeScale:
                    {
                        deactiveGameObject.transform.localScale = Vector3.zero;
                    }
                    break;
            }

            deactiveGameObject.transform.SetParent(transform, false);
        }
    }

    /// <summary>
    /// A component that simply identifies the pool that a GameObject came from
    /// </summary>
    public class UIPooledObject : MonoBehaviour
    {
        public UIObjectPool pool { get; set; }
    }
}
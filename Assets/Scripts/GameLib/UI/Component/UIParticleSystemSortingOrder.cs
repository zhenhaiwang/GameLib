using UnityEngine;

namespace GameLib
{
    [ExecuteInEditMode]
    public sealed class UIParticleSystemSortingOrder : MonoBehaviour
    {
        private enum Type
        {
            Absolute = 0,
            Relative,
        }

        [SerializeField]
        private Type m_SetType = Type.Relative;

        public int sortingOrder;

        private void OnEnable()
        {
#if UNITY_EDITOR
            var particleSystems = GetComponentsInChildren<ParticleSystem>(true);

            for (int i = 0; i < particleSystems.Length(); i++)
            {
                if (m_SetType == Type.Absolute)
                {
                    particleSystems[i].GetComponent<Renderer>().sortingOrder = sortingOrder;
                }
                else if (m_SetType == Type.Relative)
                {
                    particleSystems[i].GetComponent<Renderer>().sortingOrder += sortingOrder;
                }
            }
#endif
        }
    }
}
using UnityEngine;

namespace GameLib
{
    [ExecuteInEditMode]
    public sealed class UIMeshRendererSortingOrder : MonoBehaviour
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
            var meshRenderers = GetComponentsInChildren<MeshRenderer>(true);

            for (int i = 0; i < meshRenderers.Length(); i++)
            {
                if (m_SetType == Type.Absolute)
                {
                    meshRenderers[i].sortingOrder = sortingOrder;
                }
                else if (m_SetType == Type.Relative)
                {
                    meshRenderers[i].sortingOrder += sortingOrder;
                }
            }
#endif
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GameLib
{
    /// <summary>
    /// List function
    /// </summary>
    public static class ExtensionList
    {
        public static bool IsNullOrEmpty<T>(this List<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// This method return length of List, and return zero when List is null or empty.
        /// </summary>
        public static int Count<T>(this List<T> list)
        {
            return list != null ? list.Count : 0;
        }

        /// <summary>
        /// This method usually used in foreach loop, return itself when enumerator is not null, otherwise return an empty List.
        /// </summary>
        public static IEnumerable<T> CheckNull<T>(this IEnumerable<T> enumerator)
        {
            return enumerator == null ? new List<T>(0) : enumerator;
        }
    }

    /// <summary>
    /// ArrayList function
    /// </summary>
    public static class ExtensionArrayList
    {
        public static bool IsNullOrEmpty(this ArrayList list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// This method return length of List, and return zero when List is null or empty.
        /// </summary>
        public static int Count(this ArrayList list)
        {
            return list != null ? list.Count : 0;
        }
    }

    /// <summary>
    /// Array function
    /// </summary>
    public static class ExtensionArray
    {
        public static bool IsNullOrEmpty<T>(this T[] array)
        {
            return array == null || array.Length == 0;
        }

        /// <summary>
        /// This method return length of Array, and return zero when array is null or empty.
        /// </summary>
        public static int Length<T>(this T[] array)
        {
            return array != null ? array.Length : 0;
        }
    }

    /// <summary>
    /// UnityEngine.Component function
    /// </summary>
    public static class ExtensionComponent
    {
        public static void SetActive(this Component component, bool value)
        {
            if (component != null && component.gameObject != null)
            {
                component.gameObject.SetActive(value);
            }
        }

        public static bool activeSelf(this Component component)
        {
            if (component != null && component.gameObject != null)
            {
                return component.gameObject.activeSelf;
            }

            return false;
        }

        public static bool activeInHierarchy(this Component component)
        {
            if (component != null && component.gameObject != null)
            {
                return component.gameObject.activeInHierarchy;
            }

            return false;
        }
    }

    /// <summary>
    /// System.Action and UnityAction call
    /// </summary>
    public static class ExtensionAction
    {
        public static void Call(this Action action)
        {
            if (action != null)
            {
                action();
            }
        }

        public static void Call<T>(this Action<T> action, T parameter)
        {
            if (action != null)
            {
                action(parameter);
            }
        }

        public static void Call<T1, T2>(this Action<T1, T2> action, T1 parameter1, T2 parameter2)
        {
            if (action != null)
            {
                action(parameter1, parameter2);
            }
        }

        public static void Call<T>(this UnityAction<T> action, T parameter)
        {
            if (action != null)
            {
                action(parameter);
            }
        }
    }

    /// <summary>
    /// GameObject function
    /// </summary>
    public static class ExtensionGameObject
    {
        public static Transform FindChild(this GameObject gameObject, string name)
        {
            return gameObject.transform.Find(name);
        }

        public static GameObject AddChild(this GameObject gameObject, GameObject prefab = null, bool resetTransform = true)
        {
            if (gameObject == null)
            {
                return null;
            }

            GameObject go;

            if (prefab != null)
            {
                go = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
            }
            else
            {
                go = new GameObject();
            }

            if (go != null)
            {
                go.SetParent(gameObject, resetTransform);
            }

            return go;
        }

        public static void SetParent(this GameObject gameObject, GameObject parent, bool resetTransform = true)
        {
            if (gameObject == null || parent == null)
            {
                return;
            }

            gameObject.transform.SetParent(parent.transform, false);

            if (resetTransform)
            {
                gameObject.transform.Reset();
            }
        }

        public static void SetChild(this GameObject gameObject, GameObject child, bool worldPositionStays = true)
        {
            if (gameObject == null || child == null)
            {
                return;
            }

            child.transform.SetParent(gameObject.transform, worldPositionStays);
        }

        public static void SetLayer(this GameObject gameObject, int layer)
        {
            if (gameObject == null || layer < 0)
            {
                return;
            }

            if (!gameObject.layer.Equals(layer))
            {
                gameObject.layer = layer;
            }

            var transform = gameObject.transform;

            for (int i = 0, max = transform.childCount; i < max; i++)
            {
                transform.GetChild(i).gameObject.SetLayer(layer);
            }
        }

        public static void ResetTransform(this GameObject gameObject)
        {
            gameObject.transform.Reset();
        }
    }

    /// <summary>
    /// Transform function
    /// </summary>
    public static class ExtensionTransform
    {
        public static void Reset(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }
    }
}
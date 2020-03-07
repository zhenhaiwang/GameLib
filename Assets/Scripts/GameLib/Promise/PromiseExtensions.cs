using System;
using System.Collections;
using UnityEngine;

namespace GameLib
{
    /// <summary>
    /// Promise.Extensions
    /// </summary>
    public static class PromiseExtensions
    {
        public static Promise Then(this GameObject gameObject, IEnumerator coroutine)
        {
            return gameObject.CreatePromise().Then(coroutine);
        }

        public static Promise Then(this MonoBehaviour behaviour, IEnumerator coroutine)
        {
            return behaviour.CreatePromise().Then(coroutine);
        }

        public static Promise Then(this GameObject gameObject, Action action)
        {
            return gameObject.CreatePromise().Then(action);
        }

        public static Promise Then(this MonoBehaviour behaviour, Action action)
        {
            return behaviour.CreatePromise().Then(action);
        }

        public static Promise Then(this GameObject gameObject, Action<object> action, object parameterFirst)
        {
            return gameObject.CreatePromise().Then(action, parameterFirst);
        }

        public static Promise Then(this MonoBehaviour behaviour, Action<object> action, object parameterFirst)
        {
            return behaviour.CreatePromise().Then(action, parameterFirst);
        }

        public static Promise Then(this GameObject gameObject, Action<object, object> action, object parameterFirst, object parameterSecond)
        {
            return gameObject.CreatePromise().Then(action, parameterFirst, parameterSecond);
        }

        public static Promise Then(this MonoBehaviour behaviour, Action<object, object> action, object parameterFirst, object parameterSecond)
        {
            return behaviour.CreatePromise().Then(action, parameterFirst, parameterSecond);
        }

        public static Promise Wait(this GameObject gameObject, float time)
        {
            return gameObject.CreatePromise().Wait(time);
        }

        public static Promise Wait(this MonoBehaviour behaviour, float time)
        {
            return behaviour.CreatePromise().Wait(time);
        }

        public static Promise WaitRealtime(this GameObject gameObject, float time)
        {
            return gameObject.CreatePromise().WaitRealtime(time);
        }

        public static Promise WaitRealtime(this MonoBehaviour behaviour, float time)
        {
            return behaviour.CreatePromise().WaitRealtime(time);
        }

        public static Promise WaitOneFrame(this GameObject gameObject)
        {
            return gameObject.CreatePromise().WaitOneFrame();
        }

        public static Promise WaitOneFrame(this MonoBehaviour behaviour)
        {
            return behaviour.CreatePromise().WaitOneFrame();
        }

        private static Promise CreatePromise(this GameObject gameObject)
        {
            var promise = gameObject.GetComponent<Promise>();

            if (promise == null)
            {
                promise = gameObject.AddComponent<Promise>();
            }

            return promise;
        }

        private static Promise CreatePromise(this MonoBehaviour behaviour)
        {
            return behaviour.gameObject.CreatePromise();
        }

        public static void StopPromise(this GameObject gameObject)
        {
            var promise = gameObject.GetComponent<Promise>();

            if (promise == null)
            {
                return;
            }

            promise.Stop();
        }

        public static void StopPromise(this MonoBehaviour behaviour)
        {
            behaviour.gameObject.StopPromise();
        }

        public static void DestroyPromise(this GameObject gameObject)
        {
            var promise = gameObject.GetComponent<Promise>();

            if (promise == null)
            {
                return;
            }

            UnityEngine.Object.Destroy(promise);
        }

        public static void DestroyPromise(this MonoBehaviour behaviour)
        {
            behaviour.gameObject.DestroyPromise();
        }
    }
}
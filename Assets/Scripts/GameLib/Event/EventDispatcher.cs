using System;
using System.Collections.Generic;

namespace GameLib
{
    public static class EventDispatcher
    {
        private static Dictionary<object, Dictionary<string, Delegate>> m_EventDict = new Dictionary<object, Dictionary<string, Delegate>>();
        private static Dictionary<string, Delegate> m_GlobalEventDict = new Dictionary<string, Delegate>();

        public static void Register(string eventName, Action handler)
        {
            Register(eventName, (Delegate)handler);
        }

        public static void Register(object target, string eventName, Action handler)
        {
            Register(target, eventName, (Delegate)handler);
        }

        public static void Register<T>(string eventName, Action<T> handler)
        {
            Register(eventName, (Delegate)handler);
        }

        public static void Register<T>(object target, string eventName, Action<T> handler)
        {
            Register(target, eventName, (Delegate)handler);
        }

        public static void Register<T, U>(string eventName, Action<T, U> handler)
        {
            Register(eventName, (Delegate)handler);
        }

        public static void Register<T, U>(object target, string eventName, Action<T, U> handler)
        {
            Register(target, eventName, (Delegate)handler);
        }

        public static void Register<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            Register(eventName, (Delegate)handler);
        }

        public static void Register<T, U, V>(object target, string eventName, Action<T, U, V> handler)
        {
            Register(target, eventName, (Delegate)handler);
        }

        public static void Register<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            Register(eventName, (Delegate)handler);
        }

        public static void Register<T, U, V, W>(object target, string eventName, Action<T, U, V, W> handler)
        {
            Register(target, eventName, (Delegate)handler);
        }

        public static void Unregister(string eventName, Action handler)
        {
            Unregister(eventName, (Delegate)handler);
        }

        public static void Unregister(object target, string eventName, Action handler)
        {
            Unregister(target, eventName, (Delegate)handler);
        }

        public static void Unregister<T>(string eventName, Action<T> handler)
        {
            Unregister(eventName, (Delegate)handler);
        }

        public static void Unregister<T>(object target, string eventName, Action<T> handler)
        {
            Unregister(target, eventName, (Delegate)handler);
        }

        public static void Unregister<T, U>(string eventName, Action<T, U> handler)
        {
            Unregister(eventName, (Delegate)handler);
        }

        public static void Unregister<T, U>(object target, string eventName, Action<T, U> handler)
        {
            Unregister(target, eventName, (Delegate)handler);
        }

        public static void Unregister<T, U, V>(string eventName, Action<T, U, V> handler)
        {
            Unregister(eventName, (Delegate)handler);
        }

        public static void Unregister<T, U, V>(object target, string eventName, Action<T, U, V> handler)
        {
            Unregister(target, eventName, (Delegate)handler);
        }

        public static void Unregister<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
        {
            Unregister(eventName, (Delegate)handler);
        }

        public static void Unregister<T, U, V, W>(object target, string eventName, Action<T, U, V, W> handler)
        {
            Unregister(target, eventName, (Delegate)handler);
        }

        public static void Execute(string eventName)
        {
            var handler = GetDelegate(eventName) as Action;

            if (handler != null)
            {
                handler();
            }
        }

        public static void Execute(object target, string eventName)
        {
            var handler = GetDelegate(target, eventName) as Action;

            if (handler != null)
            {
                handler();
            }
        }

        public static void Execute<T>(string eventName, T arg1)
        {
            var handler = GetDelegate(eventName) as Action<T>;

            if (handler != null)
            {
                handler(arg1);
            }
        }

        public static void Execute<T>(object target, string eventName, T arg1)
        {
            var handler = GetDelegate(target, eventName) as Action<T>;

            if (handler != null)
            {
                handler(arg1);
            }
        }

        public static void Execute<T, U>(string eventName, T arg1, U arg2)
        {
            var handler = GetDelegate(eventName) as Action<T, U>;

            if (handler != null)
            {
                handler(arg1, arg2);
            }
        }

        public static void Execute<T, U>(object target, string eventName, T arg1, U arg2)
        {
            var handler = GetDelegate(target, eventName) as Action<T, U>;

            if (handler != null)
            {
                handler(arg1, arg2);
            }
        }

        public static void Execute<T, U, V>(string eventName, T arg1, U arg2, V arg3)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V>;

            if (handler != null)
            {
                handler(arg1, arg2, arg3);
            }
        }

        public static void Execute<T, U, V>(object target, string eventName, T arg1, U arg2, V arg3)
        {
            var handler = GetDelegate(target, eventName) as Action<T, U, V>;

            if (handler != null)
            {
                handler(arg1, arg2, arg3);
            }
        }

        public static void Execute<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            var handler = GetDelegate(eventName) as Action<T, U, V, W>;

            if (handler != null)
            {
                handler(arg1, arg2, arg3, arg4);
            }
        }

        public static void Execute<T, U, V, W>(object target, string eventName, T arg1, U arg2, V arg3, W arg4)
        {
            var handler = GetDelegate(target, eventName) as Action<T, U, V, W>;

            if (handler != null)
            {
                handler(arg1, arg2, arg3, arg4);
            }
        }

        private static void Register(string eventName, Delegate handler)
        {
            Delegate preHandlers;

            if (m_GlobalEventDict.TryGetValue(eventName, out preHandlers))
            {
                if (preHandlers == null || !Array.Exists<Delegate>(preHandlers.GetInvocationList(), element => element == handler))
                {
                    m_GlobalEventDict[eventName] = Delegate.Combine(preHandlers, handler);
                }
            }
            else
            {
                m_GlobalEventDict.Add(eventName, handler);
            }
        }

        private static void Register(object target, string eventName, Delegate handler)
        {
#if UNITY_EDITOR
            if (target == null)
            {
                Log.Error("[EventDispatcher] register event error, target object can not be null");

                return;
            }
#endif
            Dictionary<string, Delegate> handlers;

            if (!m_EventDict.TryGetValue(target, out handlers))
            {
                handlers = new Dictionary<string, Delegate>();
                m_EventDict.Add(target, handlers);
            }

            Delegate preHandlers;

            if (handlers.TryGetValue(eventName, out preHandlers))
            {
                handlers[eventName] = Delegate.Combine(preHandlers, handler);
            }
            else
            {
                handlers.Add(eventName, handler);
            }
        }

        private static void Unregister(string eventName, Delegate handler)
        {
            Delegate preHandlers;

            if (m_GlobalEventDict.TryGetValue(eventName, out preHandlers))
            {
                m_GlobalEventDict[eventName] = Delegate.Remove(preHandlers, handler);
            }
        }

        private static void Unregister(object target, string eventName, Delegate handler)
        {
#if UNITY_EDITOR
            if (target == null)
            {
                Log.Error("[EventDispatcher] unregister event error, target object can not be null");

                return;
            }
#endif
            Dictionary<string, Delegate> handlers;

            if (m_EventDict.TryGetValue(target, out handlers))
            {
                Delegate preHandlers;

                if (handlers.TryGetValue(eventName, out preHandlers))
                {
                    handlers[eventName] = Delegate.Remove(preHandlers, handler);
                }
            }
        }

        private static Delegate GetDelegate(string eventName)
        {
            Delegate handler;

            if (m_GlobalEventDict.TryGetValue(eventName, out handler))
            {
                return handler;
            }

            return null;
        }

        private static Delegate GetDelegate(object target, string eventName)
        {
            Dictionary<string, Delegate> handlers;

            if (m_EventDict.TryGetValue(target, out handlers))
            {
                Delegate handler;

                if (handlers.TryGetValue(eventName, out handler))
                {
                    return handler;
                }
            }

            return null;
        }
    }
}
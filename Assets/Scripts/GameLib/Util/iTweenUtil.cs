using System;
using UnityEngine;

namespace GameLib
{
    public static class iTweenUtil
    {
        public static void CreateTimeout(
            GameObject go,
            float time,
            Action callback)
        {
            CreateTimeout(go, time, time.ToString(), callback);
        }

        public static void CreateTimeout(
            GameObject go,
            float time,
            string name,
            Action callback)
        {
            iTween.ValueTo(go, iTween.Hash(
                "name", "iTween_Timeout_" + name,
                "time", time,
                "from", 0,
                "to", 1,
                "onupdate", (Action<object>)((x) => { }),
                "oncomplete", (Action<object>)((x) =>
                {
                    callback.Call();
                })
            ));
        }

        public static void ClearTimeout(
            GameObject go,
            string name)
        {
            iTween.StopByName(go, "iTween_Timeout_" + name);
        }

        public static void TickValue(
            GameObject go,
            float time,
            float from,
            float to,
            Action<float> update,
            Action finish)
        {
            TickValue(go, time, from, to, iTween.EaseType.linear, time.ToString(), update, finish);
        }

        public static void TickValue(
            GameObject go,
            float time,
            float from,
            float to,
            iTween.EaseType easeType,
            Action<float> update,
            Action finish)
        {
            TickValue(go, time, from, to, easeType, time.ToString(), update, finish);
        }

        public static void TickValue(
            GameObject go,
            float time,
            float from,
            float to,
            string name,
            Action<float> update,
            Action finish)
        {
            TickValue(go, time, from, to, iTween.EaseType.linear, name, update, finish);
        }

        public static void TickValue(
            GameObject go,
            float time,
            float from,
            float to,
            iTween.EaseType easeType,
            string name,
            Action<float> update,
            Action finish)
        {
            iTween.ValueTo(go, iTween.Hash(
                "name", "iTweenValueTo_" + name,
                "time", time,
                "from", from,
                "to", to,
                "easetype", easeType,
                "onupdate", (Action<object>)((x) =>
                {
                    update.Call((float)x);
                }),
                "oncomplete", (Action<object>)((x) =>
                {
                    finish.Call();
                })
            ));
        }

        public static void MoveTo(
            this GameObject go,
            Vector3 position,
            float time,
            Action callback)
        {
            MoveTo(go, position, time, iTween.EaseType.linear, false, callback);
        }

        public static void MoveTo(
            this GameObject go,
            Vector3 position,
            float time,
            iTween.EaseType easeType,
            Action callback)
        {
            MoveTo(go, position, time, easeType, false, callback);
        }

        public static void MoveTo(
            this GameObject go,
            Vector3 position,
            float time,
            iTween.EaseType easeType,
            bool local,
            Action callback)
        {
            go.SetActive(true);

            iTween.MoveTo(go, iTween.Hash(
                "position", position,
                "time", time,
                "islocal", local,
                "name", "iTween_MoveTo_" + time.ToString(),
                "easetype", easeType,
                "oncomplete", (Action<object>)((x) =>
                {
                    callback.Call();
                })
             ));
        }

        public static void MovePath(
            this GameObject go,
            Vector3[] path,
            float time,
            Action callback)
        {
            go.SetActive(true);

            iTween.MoveTo(go, iTween.Hash(
                "path", path,
                "time", time,
                "movetopath", false,
                "islocal", true,
                "name", "iTweenMovePath_" + time.ToString(),
                "easetype", iTween.EaseType.linear,
                "oncomplete", (Action<object>)((x) =>
                {
                    callback.Call();
                })
             ));
        }
    }
}
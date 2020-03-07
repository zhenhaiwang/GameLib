using System.Collections;
using UnityEngine;

namespace GameLib
{
    /// <summary>
    /// Promise.Example
    /// </summary>
    public sealed class PromiseExample : MonoBehaviour
    {
        public void OnClick()
        {
            string testString = "promise";
            double testNumber = 0;

            this.Then(DoSomething)                              // 执行方法 (无参数) 也可以这样写 gameObject.Then(DoSomething)
                .Wait(1f)                                       // 等待1秒
                .Then(DoSomething, testString)                  // 执行方法 (1个参数)
                .WaitRealtime(5f)                               // 等待5秒 (realtime)
                .Then(DoSomething, testString, testNumber)      // 执行方法 (2个参数)
                .WaitOneFrame()                                 // 等待1帧
                .Then(DoCoroutine())                            // 执行协程
                .Then(() =>                                     // 执行闭包
                {
                    Debug.Log(testString);
                });

            //this.StopPromise();
            //this.DestroyPromise();
        }

        private void DoSomething() { }

        private void DoSomething(object first) { }

        private void DoSomething(object first, object second) { }

        private IEnumerator DoCoroutine()
        {
            yield return null;
        }
    }
}
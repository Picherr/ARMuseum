using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using ARMuseum;

namespace ARMuseum.Tests.PlayMode
{
    /// <summary>
    /// 场景加载冒烟测试：逐一加载全部场景，确认能成功切换到目标场景。
    /// </summary>
    public class SceneLoadingSmokeTests
    {
        // 仅覆盖纳入 Build Settings 的应用流程场景；Scan 是建图开发场景，未进 Build Settings，故不在此列。
        private static readonly string[] SceneSequence =
        {
            SceneNames.Beginning,
            SceneNames.NanYueWenDi,
            SceneNames.MeiYuDaGuan,
            SceneNames.ShengHuoQiJu,
            SceneNames.HaiLuYangFan,
            SceneNames.Navigation,
        };

        [UnityTest]
        public IEnumerator SceneLoader_LoadsEachScene()
        {
            foreach (var name in SceneSequence)
            {
                SceneLoader.Load(name);
                yield return null;
                Assert.AreEqual(name, SceneManager.GetActiveScene().name, $"场景 {name} 未能成功加载");
            }
        }
    }
}

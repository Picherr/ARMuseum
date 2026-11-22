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
                ExpectDesktopMotionTrackingDiagnostic(name);
                SceneLoader.Load(name);
                yield return null;
                if (name == SceneNames.Navigation)
                    yield return null;
                Assert.AreEqual(name, SceneManager.GetActiveScene().name, $"场景 {name} 未能成功加载");
            }
        }

#if UNITY_EDITOR
        [UnityTest]
        public IEnumerator Scan_InitializesAndPassesSavedMapToNavigation()
        {
            ExpectDesktopMotionTrackingDiagnostic(SceneNames.Scan);
            UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(
                "Assets/Scenes/Scan.unity", new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
            yield return null;

            Assert.AreEqual(SceneNames.Scan, SceneManager.GetActiveScene().name);
            Assert.IsNotNull(UnityEngine.Object.FindObjectOfType<BuildController>());
            Assert.IsNotNull(UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>());
            var saveButton = UnityEngine.GameObject.Find("Canvas/Panel/Button").GetComponent<UnityEngine.UI.Button>();
            Assert.IsFalse(saveButton.interactable, "未获得稳定跟踪和地图名称前不能保存。");

            bool hadID = UnityEngine.PlayerPrefs.HasKey("MapID");
            bool hadName = UnityEngine.PlayerPrefs.HasKey("MapName");
            string previousID = UnityEngine.PlayerPrefs.GetString("MapID", string.Empty);
            string previousName = UnityEngine.PlayerPrefs.GetString("MapName", string.Empty);
            try
            {
                UnityEngine.PlayerPrefs.SetString("MapID", "scan-flow-test-map");
                UnityEngine.PlayerPrefs.SetString("MapName", "scan-flow-test-name");
                ExpectDesktopMotionTrackingDiagnostic(SceneNames.Navigation);
                UnityEngine.GameObject.Find("Canvas/Panel/Next").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
                yield return null;
                yield return null;

                Assert.AreEqual(SceneNames.Navigation, SceneManager.GetActiveScene().name);
                Assert.AreEqual("scan-flow-test-map", UnityEngine.GameObject.Find("Canvas/Panel/InputFieldID").GetComponent<UnityEngine.UI.InputField>().text);
                Assert.AreEqual("scan-flow-test-name", UnityEngine.GameObject.Find("Canvas/Panel/InputFieldName").GetComponent<UnityEngine.UI.InputField>().text);
                Assert.IsFalse(UnityEngine.GameObject.Find("Canvas/Panel/ButtonStart").GetComponent<UnityEngine.UI.Button>().interactable);
            }
            finally
            {
                if (hadID) UnityEngine.PlayerPrefs.SetString("MapID", previousID);
                else UnityEngine.PlayerPrefs.DeleteKey("MapID");
                if (hadName) UnityEngine.PlayerPrefs.SetString("MapName", previousName);
                else UnityEngine.PlayerPrefs.DeleteKey("MapName");
                UnityEngine.PlayerPrefs.Save();
            }
        }
#endif

        private static void ExpectDesktopMotionTrackingDiagnostic(string sceneName)
        {
            // Windows Editor 没有这些移动端运动跟踪源；仅预期 SDK 的两条关联诊断，其他错误仍使测试失败。
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor &&
                (sceneName == SceneNames.Scan || sceneName == SceneNames.Navigation))
            {
                LogAssert.Expect(UnityEngine.LogType.Error,
                    "Available frame source not found from candidates:\nAREngine ARCore ARKit MotionTracker \nThis device is not supported by all frame sources in current AR Session.");
                LogAssert.Expect(UnityEngine.LogType.Error, "Fail to Assemble: missing easyar.FrameSource");
            }
        }
    }
}

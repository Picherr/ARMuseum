using ARMuseum;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ARMuseum.Tests.EditMode
{
    public class MapSceneConfigurationTests
    {
        [TestCase("Scan")]
        [TestCase("Navigation")]
        public void MapScenes_HaveNoMissingScriptsOrPrefabs(string sceneName)
        {
            WithScene(sceneName, scene =>
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform node in root.GetComponentsInChildren<Transform>(true))
                    {
                        Assert.IsFalse(PrefabUtility.IsPrefabAssetMissing(node.gameObject), node.name + " 引用了缺失预制体。");
                        Assert.AreEqual(0, GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(node.gameObject), node.name + " 存在缺失脚本。");
                        foreach (Text text in node.GetComponents<Text>())
                            Assert.IsNotNull(text.font, text.name + " 缺少 UI 字体，文字无法显示。");
                    }
                }
                Assert.IsNotNull(FindInScene<EventSystem>(scene), "地图 UI 需要 EventSystem。");
            });
        }

        [Test]
        public void Scan_AllBuildDependenciesAndNavigationButtonAreWired()
        {
            WithScene("Scan", scene =>
            {
                BuildController controller = FindInScene<BuildController>(scene);
                Assert.IsNotNull(controller);
                var serialized = new SerializedObject(controller);
                foreach (string field in new[] { "session", "mapWorker", "map", "UIText", "btnSave", "wrController", "inputfield" })
                    Assert.IsNotNull(serialized.FindProperty(field).objectReferenceValue, "建图字段未接线：" + field);

                var save = (Button)serialized.FindProperty("btnSave").objectReferenceValue;
                Assert.AreEqual(0, save.onClick.GetPersistentEventCount(), "保存由组件生命周期接线，不能重复配置序列化回调。");
                var input = (InputField)serialized.FindProperty("inputfield").objectReferenceValue;
                Assert.IsTrue(input.gameObject.activeInHierarchy);
                Assert.IsNotNull(input.GetComponentInParent<Canvas>());
                Assert.IsNotNull(input.GetComponentInParent<GraphicRaycaster>());
                var next = FindInScene<Button>(scene, "Next");
                Assert.IsNotNull(next);
                Assert.AreEqual(1, next.onClick.GetPersistentEventCount());
                Assert.IsInstanceOf<SceneNavigation>(next.onClick.GetPersistentTarget(0));
                Assert.AreEqual("changeToNavigation", next.onClick.GetPersistentMethodName(0));
            });
        }

        [Test]
        public void Navigation_AllLocalizationControlsAreWiredWithoutAutomaticMapLoading()
        {
            WithScene("Navigation", scene =>
            {
                LocalizeController controller = FindInScene<LocalizeController>(scene);
                Assert.IsNotNull(controller);
                var serialized = new SerializedObject(controller);
                foreach (string field in new[] { "session", "mapWorker", "map", "panel", "UIText", "inputID", "inputName", "btnStart", "btnStop", "btnLoad" })
                    Assert.IsNotNull(serialized.FindProperty(field).objectReferenceValue, "定位字段未接线：" + field);

                var map = new SerializedObject(serialized.FindProperty("map").objectReferenceValue);
                Assert.IsNull(map.FindProperty("mapWorker").objectReferenceValue, "地图应在点击加载后才交给 SDK 下载。");
                Assert.AreEqual(1, map.FindProperty("SourceType").enumValueIndex, "定位地图必须来自 MapManager。");
            });
        }

        private static T FindInScene<T>(Scene scene, string objectName = null) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (T component in root.GetComponentsInChildren<T>(true))
                    if (objectName == null || component.name == objectName)
                        return component;
            return null;
        }

        private static void WithScene(string name, System.Action<Scene> assertion)
        {
            string path = "Assets/Scenes/" + name + ".unity";
            Scene originalActiveScene = SceneManager.GetActiveScene();
            Scene scene = SceneManager.GetSceneByPath(path);
            bool wasOpen = scene.IsValid() && scene.isLoaded;
            try
            {
                if (!wasOpen)
                    scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
                assertion(scene);
            }
            finally
            {
                if (!wasOpen && scene.IsValid())
                    EditorSceneManager.CloseScene(scene, true);
                if (originalActiveScene.IsValid() && originalActiveScene.isLoaded)
                    SceneManager.SetActiveScene(originalActiveScene);
            }
        }
    }
}

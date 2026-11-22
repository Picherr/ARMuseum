using System.Collections.Generic;
using System.Reflection;
using easyar;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace ARMuseum.Tests.EditMode
{
    /// <summary>
    /// 验证异步回调状态和失败恢复，不初始化 EasyAR 原生组件或访问地图服务。
    /// </summary>
    public class SpatialMapLifecycleTests
    {
        private readonly List<GameObject> objects = new List<GameObject>();
        private bool hadID;
        private bool hadName;
        private string previousID;
        private string previousName;

        [SetUp]
        public void RememberMapPreferences()
        {
            hadID = PlayerPrefs.HasKey("MapID");
            hadName = PlayerPrefs.HasKey("MapName");
            previousID = PlayerPrefs.GetString("MapID", string.Empty);
            previousName = PlayerPrefs.GetString("MapName", string.Empty);
            PlayerPrefs.SetString("MapID", "previous-map");
            PlayerPrefs.SetString("MapName", "previous-name");
        }

        [TearDown]
        public void CleanUp()
        {
            foreach (GameObject item in objects)
                if (item) Object.DestroyImmediate(item);
            objects.Clear();

            if (hadID) PlayerPrefs.SetString("MapID", previousID);
            else PlayerPrefs.DeleteKey("MapID");
            if (hadName) PlayerPrefs.SetString("MapName", previousName);
            else PlayerPrefs.DeleteKey("MapName");
            PlayerPrefs.Save();
        }

        [Test]
        public void HostSuccess_PersistsCompletedMapAndFinishesSaving()
        {
            BuildController controller = CreateInactive<BuildController>();
            SetField(controller, "isSaving", true);
            Invoke(controller, "SaveMapHostBack", new SparseSpatialMapController.SparseSpatialMapInfo
            {
                ID = "completed-map", Name = "completed-name"
            }, true, string.Empty);

            Assert.AreEqual("completed-map", PlayerPrefs.GetString("MapID"));
            Assert.AreEqual("completed-name", PlayerPrefs.GetString("MapName"));
            Assert.IsFalse((bool)GetField(controller, "isSaving"));
            Assert.IsTrue((bool)GetField(controller, "hasSaved"));
        }

        [TestCase(false, "failed-map")]
        [TestCase(true, "")]
        public void HostFailureOrMissingID_PreservesPreviouslySavedMap(bool success, string id)
        {
            BuildController controller = CreateInactive<BuildController>();
            SetField(controller, "isSaving", true);
            Invoke(controller, "SaveMapHostBack", new SparseSpatialMapController.SparseSpatialMapInfo
            {
                ID = id, Name = "new-name"
            }, success, "service-failure");

            Assert.AreEqual("previous-map", PlayerPrefs.GetString("MapID"));
            Assert.AreEqual("previous-name", PlayerPrefs.GetString("MapName"));
            Assert.IsFalse((bool)GetField(controller, "isSaving"));
            Assert.IsFalse((bool)GetField(controller, "hasSaved"));
        }

        [Test]
        public void SuccessfulLoad_DoesNotAutomaticallyStartLocalization()
        {
            LocalizeController controller = CreateInactive<LocalizeController>();
            SparseSpatialMapController map = CreateInactive<SparseSpatialMapController>();
            SetField(controller, "map", map);
            SetState(controller, "Loading");

            Invoke(controller, "MapLoadBack", map, new SparseSpatialMapController.SparseSpatialMapInfo
            {
                ID = "loaded-map", Name = "loaded-name"
            }, true, string.Empty);

            Assert.AreEqual("Loaded", GetField(controller, "state").ToString());
            Assert.IsFalse((bool)GetField(controller, "isLocalizing"));
        }

        [Test]
        public void FailedLoadCleanup_AllowsRetryEvenWhenNativeMapWasNotLoaded()
        {
            LocalizeController controller = CreateInactive<LocalizeController>();
            SetState(controller, "Unloading");
            SetField(controller, "unloadingFailedLoad", true);

            Invoke(controller, "MapUnloadBack", null, false, "map-not-loaded");

            Assert.AreEqual("Idle", GetField(controller, "state").ToString());
            Assert.IsFalse((bool)GetField(controller, "isLocalizing"));
        }

        [Test]
        public void UnloadRetry_ClearsSDKLoaderLeftBySynchronousUnloadException()
        {
            LocalizeController controller = CreateInactive<LocalizeController>();
            SparseSpatialMapController map = CreateInactive<SparseSpatialMapController>();
            SparseSpatialMapWorkerFrameFilter worker = CreateInactive<SparseSpatialMapWorkerFrameFilter>();
            SparseSpatialMapHolder holder = CreateInactive<SparseSpatialMapHolder>();
            SetField(worker, "<MapHolder>k__BackingField", holder);
            SetField(map, "<MapInfo>k__BackingField", new SparseSpatialMapController.SparseSpatialMapInfo
            {
                ID = "old-map", Name = "old-name"
            });
            map.SourceType = SparseSpatialMapController.DataSource.MapManager;
            // 模拟 SDK 已从 Holder 移除地图，但同步异常让 loader 没有被清空。
            SetField(map, "loader", worker);
            SetField(controller, "map", map);
            SetField(controller, "mapWorker", worker);
            SetState(controller, "UnloadFailed");

            Invoke(controller, "RetryUnload");

            Assert.IsNull(GetField(map, "loader"), "遗留 loader 会让下次赋值同 worker 时跳过加载。");
            // 原生 Localizer 没有创建，恢复请求仍保持为可重试状态。
            Assert.AreEqual("UnloadFailed", GetField(controller, "state").ToString());
        }

        [Test]
        public void StartButton_BeforeLoadingShowsLoadPromptWithoutStartingNativeLocalization()
        {
            LocalizeController controller = CreateLocalizationUI();
            Text status = (Text)GetField(controller, "UIText");
            Button start = (Button)GetField(controller, "btnStart");
            Button stop = (Button)GetField(controller, "btnStop");

            Assert.IsFalse(start.interactable);
            // UnityEvent.Invoke 可模拟被旧序列化事件或代码强制触发的禁用按钮。
            Assert.DoesNotThrow(() => start.onClick.Invoke());

            Assert.AreEqual("请先成功加载当前地图，再开始定位。", status.text);
            Assert.IsFalse(stop.interactable);
        }

        [TestCase("", "map-name")]
        [TestCase("map-id", "")]
        [TestCase("  ", "  ")]
        public void LoadButton_RequiresBothMapIDAndNameBeforeCallingService(string id, string name)
        {
            LocalizeController controller = CreateLocalizationUI();
            ((InputField)GetField(controller, "inputID")).text = id;
            ((InputField)GetField(controller, "inputName")).text = name;

            Assert.DoesNotThrow(() => ((Button)GetField(controller, "btnLoad")).onClick.Invoke());

            Assert.AreEqual("请填写地图 ID 和名称。", ((Text)GetField(controller, "UIText")).text);
            Assert.IsFalse(((Button)GetField(controller, "btnStart")).interactable);
        }

        [Test]
        public void LoadButton_WhenWorkerIsMissingShowsReadinessPromptWithoutThrowing()
        {
            LocalizeController controller = CreateLocalizationUI();
            ((InputField)GetField(controller, "inputID")).text = "map-id";
            ((InputField)GetField(controller, "inputName")).text = "map-name";
            SetField(controller, "mapWorker", null);

            Assert.DoesNotThrow(() => ((Button)GetField(controller, "btnLoad")).onClick.Invoke());

            Assert.AreEqual("地图服务尚未准备好，请等待 EasyAR 初始化后重试。",
                ((Text)GetField(controller, "UIText")).text);
        }

        [TestCase("")]
        [TestCase("   ")]
        public void SaveButton_RejectsEmptyOrWhitespaceName(string name)
        {
            BuildController controller = CreateBuildUI();
            ((InputField)GetField(controller, "inputfield")).text = name;
            Button save = (Button)GetField(controller, "btnSave");

            Assert.IsFalse(save.interactable);
            Assert.DoesNotThrow(() => save.onClick.Invoke());

            Assert.AreEqual("请先填写地图名称。", ((Text)GetField(controller, "UIText")).text);
            Assert.AreEqual("previous-map", PlayerPrefs.GetString("MapID"));
        }

        [Test]
        public void SaveButton_DuringPendingUploadIgnoresRepeatedForcedClicks()
        {
            BuildController controller = CreateBuildUI();
            ((InputField)GetField(controller, "inputfield")).text = "map-name";
            SetField(controller, "isSaving", true);
            Invoke(controller, "RefreshSaveButton");
            Text status = (Text)GetField(controller, "UIText");
            status.text = "正在保存地图，请稍候。";
            Button save = (Button)GetField(controller, "btnSave");

            Assert.IsFalse(save.interactable);
            Assert.DoesNotThrow(() =>
            {
                save.onClick.Invoke();
                save.onClick.Invoke();
            });

            // 若再次进入保存流程，会因未初始化的 SDK Builder 改成“建图尚未准备好”。
            Assert.AreEqual("正在保存地图，请稍候。", status.text);
            Assert.IsFalse(save.interactable);
            Assert.AreEqual("previous-map", PlayerPrefs.GetString("MapID"));
        }

        [Test]
        public void SaveButton_WithoutTrackingRejectsForcedClick()
        {
            BuildController controller = CreateBuildUI();
            ((InputField)GetField(controller, "inputfield")).text = "map-name";
            Button save = (Button)GetField(controller, "btnSave");

            Assert.IsFalse(save.interactable);
            Assert.DoesNotThrow(() => save.onClick.Invoke());

            Assert.AreEqual("请在进入跟踪状态后保存地图。", ((Text)GetField(controller, "UIText")).text);
        }

        private BuildController CreateBuildUI()
        {
            BuildController controller = CreateInactive<BuildController>();
            SetField(controller, "session", CreateInactive<ARSession>());
            SetField(controller, "mapWorker", CreateInactive<SparseSpatialMapWorkerFrameFilter>());
            SetField(controller, "map", CreateInactive<SparseSpatialMapController>());
            SetField(controller, "wrController", CreateInactive<WorldRootController>());
            SetField(controller, "UIText", CreateInactive<Text>());
            SetField(controller, "btnSave", CreateInactive<Button>());
            SetField(controller, "inputfield", CreateInactive<InputField>());
            ActivateUI(controller);
            return controller;
        }

        private LocalizeController CreateLocalizationUI()
        {
            LocalizeController controller = CreateInactive<LocalizeController>();
            SetField(controller, "session", CreateInactive<ARSession>());
            SetField(controller, "mapWorker", CreateInactive<SparseSpatialMapWorkerFrameFilter>());
            SetField(controller, "map", CreateInactive<SparseSpatialMapController>());
            SetField(controller, "UIText", CreateInactive<Text>());
            SetField(controller, "inputID", CreateInactive<InputField>());
            SetField(controller, "inputName", CreateInactive<InputField>());
            SetField(controller, "btnLoad", CreateInactive<Button>());
            SetField(controller, "btnStart", CreateInactive<Button>());
            SetField(controller, "btnStop", CreateInactive<Button>());
            ActivateUI(controller);
            return controller;
        }

        private static void ActivateUI(MonoBehaviour controller)
        {
            controller.gameObject.SetActive(true);
            // EditMode 下普通组件未必自动收到生命周期回调；SDK 对象始终保持 inactive。
            if (!(bool)GetField(controller, "hasBindings"))
            {
                Invoke(controller, "Awake");
                Invoke(controller, "OnEnable");
            }
        }

        private T CreateInactive<T>() where T : Component
        {
            GameObject item = new GameObject(typeof(T).Name + " test");
            item.SetActive(false);
            objects.Add(item);
            return item.AddComponent<T>();
        }

        private static void SetState(LocalizeController controller, string state)
        {
            FieldInfo field = typeof(LocalizeController).GetField("state", BindingFlags.Instance | BindingFlags.NonPublic);
            field.SetValue(controller, System.Enum.Parse(field.FieldType, state));
        }

        private static object GetField(object target, string field)
        {
            return target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(target);
        }

        private static void SetField(object target, string field, object value)
        {
            target.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }

        private static void Invoke(object target, string method, params object[] arguments)
        {
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(target, arguments);
        }
    }
}

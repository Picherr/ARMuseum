using System;
using System.Collections;
using easyar;
using UnityEngine;
using UnityEngine.UI;

namespace ARMuseum
{
    public class LocalizeController : MonoBehaviour
    {
        [SerializeField] private ARSession session;
        [SerializeField] private SparseSpatialMapWorkerFrameFilter mapWorker;
        [SerializeField] private SparseSpatialMapController map;
        [SerializeField] private Transform panel;
        [SerializeField] private Text UIText;
        [SerializeField] private InputField inputID;
        [SerializeField] private InputField inputName;
        [SerializeField] private Button btnStart;
        [SerializeField] private Button btnStop;
        [SerializeField] private Button btnLoad;

        private enum MapState { Idle, Initializing, Loading, Loaded, Unloading, UnloadFailed }

        private MapState state;
        private bool hasBindings;
        private bool isLocalizing;
        private bool loadAfterUnload;
        private bool unloadingFailedLoad;
        private string requestedID;
        private string requestedName;
        private Coroutine initializeRoutine;

        private void Awake()
        {
            if (!session) session = FindObjectOfType<ARSession>();
            if (!mapWorker) mapWorker = FindObjectOfType<SparseSpatialMapWorkerFrameFilter>();
            if (!map) map = FindObjectOfType<SparseSpatialMapController>();

            hasBindings = session && mapWorker && map && UIText && inputID && inputName &&
                btnStart && btnStop && btnLoad;
            if (!hasBindings)
            {
                SetStatus("定位界面配置不完整，请检查场景引用。");
                Debug.LogError("[Navigation] LocalizeController 缺少必要的场景引用。", this);
                return;
            }

            // SDK 只在 Start 中从 MapManagerSource 创建 MapInfo。
            // 延迟该 Start，直到用户明确选择地图；不要在场景启动时自动下载或定位。
            map.enabled = false;
            map.MapWorker = null;
            map.SourceType = SparseSpatialMapController.DataSource.MapManager;
            if (map.MapManagerSource == null)
                map.MapManagerSource = new SparseSpatialMapController.MapManagerSourceData();

            if (string.IsNullOrWhiteSpace(inputID.text))
            {
                string savedID = PlayerPrefs.GetString("MapID", string.Empty);
                inputID.text = string.IsNullOrWhiteSpace(savedID) ? map.MapManagerSource.ID : savedID;
                inputName.text = string.IsNullOrWhiteSpace(savedID)
                    ? map.MapManagerSource.Name : PlayerPrefs.GetString("MapName", string.Empty);
            }

            // Worker.MapLoad 先于 Controller.MapLoad：失败时可在 SDK 清空 loader 前卸载，
            // 同时清掉 MapHolder 中的失败记录，后续重试和切换 ID 才不会保留旧地图。
            mapWorker.MapLoad += MapLoadBack;
            map.MapUnload += MapUnloadBack;
            map.MapLocalized += LocalizeMap;
            map.MapStopLocalize += StopLocalizeMap;
            SetStatus("确认地图 ID 和名称，点击加载地图后再开始定位。");
        }

        private void OnEnable()
        {
            if (hasBindings)
            {
                btnLoad.onClick.AddListener(LoadMap);
                btnStart.onClick.AddListener(StartLocalization);
                btnStop.onClick.AddListener(StopLocalization);
                inputID.onValueChanged.AddListener(OnMapInputChanged);
                inputName.onValueChanged.AddListener(OnMapInputChanged);
            }
            RefreshButtons();
        }

        private void OnDisable()
        {
            if (btnLoad) btnLoad.onClick.RemoveListener(LoadMap);
            if (btnStart) btnStart.onClick.RemoveListener(StartLocalization);
            if (btnStop) btnStop.onClick.RemoveListener(StopLocalization);
            if (inputID) inputID.onValueChanged.RemoveListener(OnMapInputChanged);
            if (inputName) inputName.onValueChanged.RemoveListener(OnMapInputChanged);
            if (initializeRoutine != null)
            {
                StopCoroutine(initializeRoutine);
                initializeRoutine = null;
                state = MapState.Idle;
            }
            StopLocalization();
            RefreshButtons();
        }

        private void OnDestroy()
        {
            if (mapWorker) mapWorker.MapLoad -= MapLoadBack;
            if (map)
            {
                map.MapUnload -= MapUnloadBack;
                map.MapLocalized -= LocalizeMap;
                map.MapStopLocalize -= StopLocalizeMap;
            }
        }

        public void LoadMap()
        {
            if (!isActiveAndEnabled || !hasBindings || IsBusy()) return;
            requestedID = inputID.text.Trim();
            requestedName = inputName.text.Trim();
            if (string.IsNullOrEmpty(requestedID) || string.IsNullOrEmpty(requestedName))
            {
                SetStatus("请填写地图 ID 和名称。");
                return;
            }
            if (!IsWorkerReady())
            {
                SetStatus("地图服务尚未准备好，请等待 EasyAR 初始化后重试。");
                return;
            }

            StopLocalization();
            if (state == MapState.UnloadFailed)
            {
                RetryUnload();
            }
            else if (map.MapWorker)
            {
                BeginUnload(true, false);
            }
            else
            {
                BeginLoad();
            }
        }

        private void BeginLoad()
        {
            map.MapManagerSource.ID = requestedID;
            map.MapManagerSource.Name = requestedName;
            if (map.MapInfo == null)
            {
                state = MapState.Initializing;
                SetStatus("正在初始化地图。");
                RefreshButtons();
                map.enabled = true;
                initializeRoutine = StartCoroutine(LoadAfterMapStart());
                return;
            }

            // 前一张地图已完成卸载，才能修改 SDK 用来索引 MapHolder 的 ID。
            map.MapInfo.ID = requestedID;
            map.MapInfo.Name = requestedName;
            state = MapState.Loading;
            SetStatus("正在加载地图，请稍候。");
            RefreshButtons();
            try
            {
                map.MapWorker = mapWorker;
            }
            catch (Exception)
            {
                state = MapState.Idle;
                map.MapWorker = null;
                SetStatus("地图加载失败，请检查服务配置和网络后重试。");
                RefreshButtons();
            }
        }

        private IEnumerator LoadAfterMapStart()
        {
            yield return null;
            initializeRoutine = null;
            if (map && map.MapInfo != null && isActiveAndEnabled)
            {
                BeginLoad();
            }
            else
            {
                state = MapState.Idle;
                SetStatus("地图初始化失败，请检查地图组件是否启用后重试。");
                RefreshButtons();
            }
        }

        private void MapLoadBack(SparseSpatialMapController controller,
            SparseSpatialMapController.SparseSpatialMapInfo mapInfo, bool isSuccess, string error)
        {
            if (controller != map || state != MapState.Loading) return;
            if (isSuccess)
            {
                state = MapState.Loaded;
                SetStatus("地图加载成功，可以开始定位。");
                RefreshButtons();
            }
            else
            {
                BeginUnload(false, true);
            }
        }

        private void BeginUnload(bool continueLoading, bool failedLoad)
        {
            loadAfterUnload = continueLoading;
            unloadingFailedLoad = failedLoad;
            state = MapState.Unloading;
            SetStatus(failedLoad ? "地图加载失败，正在清理，请稍后重试。" : "正在切换地图，请稍候。");
            RefreshButtons();
            try
            {
                map.MapWorker = null;
            }
            catch (Exception)
            {
                state = MapState.UnloadFailed;
                SetStatus("地图释放失败，请点击加载地图重试。");
                RefreshButtons();
            }
        }

        private void MapUnloadBack(SparseSpatialMapController.SparseSpatialMapInfo mapInfo,
            bool isSuccess, string error)
        {
            CompleteUnload(isSuccess);
        }

        private void CompleteUnload(bool isSuccess)
        {
            if (state != MapState.Unloading) return;
            // 加载失败时并没有可定位的原生地图；MapHolder 已在卸载请求前移除。
            if (!isSuccess && !unloadingFailedLoad)
            {
                state = MapState.UnloadFailed;
                SetStatus("地图释放失败，请点击加载地图重试。");
                RefreshButtons();
                return;
            }
            state = MapState.Idle;
            if (loadAfterUnload && isActiveAndEnabled)
                BeginLoad();
            else
            {
                SetStatus(unloadingFailedLoad ? "地图加载失败，请检查 ID、服务配置和网络后重试。" : "地图已释放，请重新加载。");
                RefreshButtons();
            }
        }

        private void RetryUnload()
        {
            state = MapState.Unloading;
            loadAfterUnload = true;
            RefreshButtons();
            try
            {
                // 若上次卸载同步抛异常，SDK 已 Unhold，但还未来得及将 loader 清空。
                // 再次断开先清掉该引用，防止后续同 worker 被误判为已加载而跳过加载。
                map.MapWorker = null;
                // SDK 已移除 Holder 记录，失败重试直接请求原生地图释放。
                mapWorker.Localizer.unloadMap(map.MapInfo.ID, EasyARController.Scheduler, (Action<bool>)(success =>
                {
                    if (this) CompleteUnload(success);
                }));
            }
            catch (Exception)
            {
                state = MapState.UnloadFailed;
                SetStatus("地图释放失败，请检查服务状态后重试。");
                RefreshButtons();
            }
        }

        public void StartLocalization()
        {
            if (!isActiveAndEnabled || !hasBindings || isLocalizing) return;
            if (state != MapState.Loaded || !InputsMatchLoadedMap() || !IsWorkerReady())
            {
                SetStatus("请先成功加载当前地图，再开始定位。");
                return;
            }
            try
            {
                isLocalizing = mapWorker.Localizer.startLocalization();
                SetStatus(isLocalizing ? "正在定位，请缓慢移动设备。" : "定位未能启动，请检查设备跟踪状态后重试。");
            }
            catch (Exception)
            {
                isLocalizing = false;
                SetStatus("定位未能启动，请检查设备状态后重试。");
            }
            RefreshButtons();
        }

        public void StopLocalization()
        {
            if (!isLocalizing) return;
            isLocalizing = false;
            try
            {
                if (mapWorker && mapWorker.Localizer != null) mapWorker.Localizer.stopLocalization();
                SetStatus("地图定位已停止。");
            }
            catch (Exception)
            {
                SetStatus("定位服务已停止响应，请返回后重试。");
            }
            RefreshButtons();
        }

        private void LocalizeMap()
        {
            if (isLocalizing) SetStatus("地图定位成功。");
        }

        private void StopLocalizeMap()
        {
            if (isLocalizing) SetStatus("定位暂时丢失，正在重新定位。");
        }

        private bool IsWorkerReady()
        {
            return mapWorker && mapWorker.Localizer != null && mapWorker.Manager != null;
        }

        private bool IsBusy()
        {
            return state == MapState.Initializing || state == MapState.Loading || state == MapState.Unloading;
        }

        private bool InputsMatchLoadedMap()
        {
            return map && map.MapInfo != null && inputID && inputName &&
                inputID.text.Trim() == map.MapInfo.ID && inputName.text.Trim() == map.MapInfo.Name;
        }

        private void OnMapInputChanged(string value)
        {
            RefreshButtons();
        }

        private void RefreshButtons()
        {
            bool available = isActiveAndEnabled && hasBindings;
            if (btnLoad) btnLoad.interactable = available && !IsBusy() && !isLocalizing;
            if (btnStart) btnStart.interactable = available && state == MapState.Loaded &&
                !isLocalizing && InputsMatchLoadedMap();
            if (btnStop) btnStop.interactable = available && isLocalizing;
            if (inputID) inputID.interactable = available && !IsBusy() && !isLocalizing;
            if (inputName) inputName.interactable = available && !IsBusy() && !isLocalizing;
        }

        private void SetStatus(string message)
        {
            if (UIText) UIText.text = message;
        }
    }
}

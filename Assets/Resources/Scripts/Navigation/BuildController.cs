using System;
using easyar;
using UnityEngine;
using UnityEngine.UI;

namespace ARMuseum
{
    public class BuildController : MonoBehaviour
    {
        [SerializeField] private ARSession session;
        [SerializeField] private SparseSpatialMapWorkerFrameFilter mapWorker;
        [SerializeField] private SparseSpatialMapController map;
        [SerializeField] private Text UIText;
        [SerializeField] private Button btnSave;
        [SerializeField] private WorldRootController wrController;
        [SerializeField] private InputField inputfield;

        private SparseSpatialMapController hostingMap;
        private bool isSaving;
        private bool hasSaved;
        private bool isTracking;
        private bool hasBindings;

        private void Awake()
        {
            if (!session) session = FindObjectOfType<ARSession>();
            if (!mapWorker) mapWorker = FindObjectOfType<SparseSpatialMapWorkerFrameFilter>();
            if (!map) map = FindObjectOfType<SparseSpatialMapController>();
            if (!wrController) wrController = FindObjectOfType<WorldRootController>();
        }

        private void OnEnable()
        {
            hasBindings = session && mapWorker && map && wrController && UIText && btnSave && inputfield;
            if (!hasBindings)
            {
                SetStatus("建图界面配置不完整，请检查场景引用。");
                Debug.LogError("[Scan] BuildController 缺少必要的场景引用。", this);
                RefreshSaveButton();
                return;
            }

            btnSave.onClick.AddListener(Save);
            inputfield.onValueChanged.AddListener(OnNameChanged);
            wrController.TrackingStatusChanged += OnTrackingStatusChanged;
            isTracking = wrController.TrackingStatus == MotionTrackingStatus.Tracking;
            SetStatus(isSaving ? "正在保存地图，请稍候。" : "请输入地图名称，在跟踪稳定后保存。");
            RefreshSaveButton();
        }

        private void OnDisable()
        {
            if (btnSave) btnSave.onClick.RemoveListener(Save);
            if (inputfield) inputfield.onValueChanged.RemoveListener(OnNameChanged);
            if (wrController) wrController.TrackingStatusChanged -= OnTrackingStatusChanged;
            if (btnSave) btnSave.interactable = false;
        }

        private void OnDestroy()
        {
            UnsubscribeHost();
        }

        private void Save()
        {
            if (!isActiveAndEnabled || !hasBindings || isSaving || hasSaved) return;

            string mapName = inputfield.text.Trim();
            if (string.IsNullOrEmpty(mapName))
            {
                SetStatus("请先填写地图名称。");
                RefreshSaveButton();
                return;
            }
            if (!isTracking)
            {
                SetStatus("请在进入跟踪状态后保存地图。");
                return;
            }
            if (mapWorker.Builder == null || !mapWorker.BuilderMapController ||
                mapWorker.BuilderMapController != map || map.MapInfo == null)
            {
                SetStatus("建图尚未准备好，请等待 EasyAR 初始化并继续扫描。");
                return;
            }

            hostingMap = mapWorker.BuilderMapController;
            hostingMap.MapHost += SaveMapHostBack;
            isSaving = true;
            inputfield.interactable = false;
            RefreshSaveButton();
            SetStatus("正在保存地图，请稍候。");
            try
            {
                hostingMap.Host(mapName, null);
            }
            catch (Exception)
            {
                // SDK 错误可能带服务信息；界面和日志仅提供操作上下文。
                UnsubscribeHost();
                isSaving = false;
                inputfield.interactable = true;
                SetStatus("地图保存失败，请检查服务配置和网络后重试。");
                RefreshSaveButton();
            }
        }

        private void SaveMapHostBack(SparseSpatialMapController.SparseSpatialMapInfo mapInfo,
            bool isSuccess, string error)
        {
            UnsubscribeHost();
            isSaving = false;
            hasSaved = isSuccess && mapInfo != null && !string.IsNullOrEmpty(mapInfo.ID);
            if (hasSaved)
            {
                PlayerPrefs.SetString("MapID", mapInfo.ID);
                PlayerPrefs.SetString("MapName", mapInfo.Name ?? string.Empty);
                PlayerPrefs.Save();
                SetStatus("地图保存成功，可返回导航界面加载地图。");
            }
            else
            {
                SetStatus("地图保存失败，请检查服务配置和网络后重试。");
            }
            if (inputfield) inputfield.interactable = !hasSaved;
            RefreshSaveButton();
        }

        private void UnsubscribeHost()
        {
            if (hostingMap) hostingMap.MapHost -= SaveMapHostBack;
            hostingMap = null;
        }

        private void OnTrackingStatusChanged(MotionTrackingStatus status)
        {
            // EasyAR 在事件返回后才更新 TrackingStatus，使用事件参数判断。
            isTracking = status == MotionTrackingStatus.Tracking;
            if (!isSaving && !hasSaved)
                SetStatus(isTracking ? "跟踪稳定，可以保存地图。" : "跟踪尚未稳定，请继续扫描。");
            RefreshSaveButton();
        }

        private void OnNameChanged(string value)
        {
            RefreshSaveButton();
        }

        private void RefreshSaveButton()
        {
            if (btnSave)
                btnSave.interactable = isActiveAndEnabled && hasBindings && isTracking &&
                    !isSaving && !hasSaved && inputfield && !string.IsNullOrWhiteSpace(inputfield.text);
        }

        private void SetStatus(string message)
        {
            if (UIText) UIText.text = message;
        }
    }
}

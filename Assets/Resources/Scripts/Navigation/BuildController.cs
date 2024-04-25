using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using easyar;
using UnityEngine.UI;
using System;

public class BuildController : MonoBehaviour
{
    private ARSession session;
    private SparseSpatialMapWorkerFrameFilter mapWorker;
    private SparseSpatialMapController map;
    private Text UIText;
    private Button btnSave;
    private WorldRootController wrController;
    private InputField inputfield;

    // Start is called before the first frame update
    void Start()
    {
        session = FindObjectOfType<ARSession>();
        mapWorker = FindObjectOfType<SparseSpatialMapWorkerFrameFilter>();
        map = FindObjectOfType<SparseSpatialMapController>();
        wrController = FindObjectOfType<WorldRootController>();
        inputfield = FindObjectOfType<InputField>();

        //btnSave = FindObjectOfType<Button>();
        btnSave = GameObject.Find("/Canvas/Panel/Button").GetComponent<Button>();
        UIText = GameObject.Find("/Canvas/Panel/Text").GetComponent<Text>();

        btnSave.onClick.AddListener(Save);
        btnSave.interactable = false;
        wrController.TrackingStatusChanged += OnTrackingStatusChanged;
        Debug.Log("准备就绪。");
    }

    /// <summary>
    /// 保存地图
    /// </summary>
    private void Save()
    {
        Debug.Log("进入save函数。");
        mapWorker.BuilderMapController.MapHost += SaveMapHostBack;
        try
        {
            Debug.Log("开始保存。");
            mapWorker.BuilderMapController.Host(inputfield.text, null);
            Debug.Log("Host开始工作。");
            UIText.text = "开始保存地图。";
        }
        catch (Exception ex)
        {
            Debug.Log("保存出错。");
            UIText.text = "保存出错。" + ex.Message;
        }
    }

    private void SaveMapHostBack(SparseSpatialMapController.SparseSpatialMapInfo mapInfo, bool isSuccess, string error)
    {
        if (isSuccess)
        {
            PlayerPrefs.SetString("MapID", mapInfo.ID);
            PlayerPrefs.SetString("MapName", mapInfo.Name);
            UIText.text = "地图保存成功。\r\nMapID:" + mapInfo.ID;
        }
        else
        {
            UIText.text = "地图保存出错。" + error;
        }
    }

    private void OnTrackingStatusChanged(MotionTrackingStatus status)
    {
        if (status == MotionTrackingStatus.Tracking)
        {
            UIText.text = "进入跟踪状态。";
            btnSave.interactable = true;
        }
        else
        {
            UIText.text = "退出跟踪状态。" + status.ToString();
            btnSave.interactable = false;
        }
    }
}

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
        Debug.Log("׼��������");
    }

    /// <summary>
    /// �����ͼ
    /// </summary>
    private void Save()
    {
        Debug.Log("����save������");
        mapWorker.BuilderMapController.MapHost += SaveMapHostBack;
        try
        {
            Debug.Log("��ʼ���档");
            mapWorker.BuilderMapController.Host(inputfield.text, null);
            Debug.Log("Host��ʼ������");
            UIText.text = "��ʼ�����ͼ��";
        }
        catch (Exception ex)
        {
            Debug.Log("�������");
            UIText.text = "�������" + ex.Message;
        }
    }

    private void SaveMapHostBack(SparseSpatialMapController.SparseSpatialMapInfo mapInfo, bool isSuccess, string error)
    {
        if (isSuccess)
        {
            PlayerPrefs.SetString("MapID", mapInfo.ID);
            PlayerPrefs.SetString("MapName", mapInfo.Name);
            UIText.text = "��ͼ����ɹ���\r\nMapID:" + mapInfo.ID;
        }
        else
        {
            UIText.text = "��ͼ�������" + error;
        }
    }

    private void OnTrackingStatusChanged(MotionTrackingStatus status)
    {
        if (status == MotionTrackingStatus.Tracking)
        {
            UIText.text = "�������״̬��";
            btnSave.interactable = true;
        }
        else
        {
            UIText.text = "�˳�����״̬��" + status.ToString();
            btnSave.interactable = false;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using easyar;
using System;
using UnityEngine.UI;

public class LocalizeController : MonoBehaviour
{
    private ARSession session;
    private SparseSpatialMapWorkerFrameFilter mapWorker;
    private SparseSpatialMapController map;

    private Text UIText;
    private InputField inputID;
    private InputField inputName;

    private Button btnStart;
    private Button btnStop;
    private Button btnLoad;

    // Start is called before the first frame update
    void Start()
    {
        session = FindObjectOfType<ARSession>();
        mapWorker = FindObjectOfType<SparseSpatialMapWorkerFrameFilter>();
        map = FindObjectOfType<SparseSpatialMapController>();

        Transform panel = GameObject.Find("/Canvas/Panel").transform;

        UIText = panel.Find("Text").GetComponent<Text>();
        inputID = panel.Find("InputFieldID").GetComponent<InputField>();
        inputName = panel.Find("InputFieldName").GetComponent<InputField>();
        btnStart = panel.Find("ButtonStart").GetComponent<Button>();
        btnStop = panel.Find("ButtonStop").GetComponent<Button>();
        btnLoad = panel.Find("Load").GetComponent<Button>();

        btnStart.onClick.AddListener(StartLocalization);
        btnStop.onClick.AddListener(StopLocalization);
        //btnLoad.onClick.AddListener(LoadMap);

        map.MapLoad += MapLoadBack;
        map.MapLocalized += LocalizeMap;
        map.MapStopLocalize += StopLocalizeMap;

        if (inputID.text.Length < 0)
        {
            inputID.text = PlayerPrefs.GetString("MapID", "");
            inputName.text = PlayerPrefs.GetString("MapName", "");
            Debug.Log("inputField�ѻ�ȡstring");
        }

        StartLocalization();
    }

    public void StartLocalization()
    {
        if(inputID.text.Length>0)
        {
            map.MapManagerSource.ID = inputID.text;
            map.MapManagerSource.Name = inputName.text;
        }

        UIText.text = "��ʼ���ػ���ͼ��";
        mapWorker.Localizer.startLocalization();
    }

    public void StopLocalization()
    {
        mapWorker.Localizer.stopLocalization();
    }

    private void MapLoadBack(SparseSpatialMapController.SparseSpatialMapInfo mapInfo,bool isSuccess, string error)
    {
        if(isSuccess)
        {
            UIText.text = "��ͼ��" + mapInfo.Name + "���سɹ���";
        }
        else
        {
            UIText.text = "��ͼ����ʧ�ܡ�" + error;
        }
    }

    private void LocalizeMap()
    {
        UIText.text = "��ͼ��λ�ɹ���" + DateTime.Now.ToShortTimeString();
    }

    private void StopLocalizeMap()
    {
        UIText.text = "��ͼֹͣ��λ��" + DateTime.Now.ToShortTimeString();
    }
}

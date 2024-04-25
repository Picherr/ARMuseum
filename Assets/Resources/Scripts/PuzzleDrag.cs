using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using Unity.VisualScripting;

public class PuzzleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private GameObject ParentWhenDrag;
    private GameObject ParentInitial;

    private YPController ypController;

    private Vector3 touchOffset; // ��ק��ƫ����
    private Transform panel; // �����е�Panel��������ק�����еĸ�����
    private ScrollRect scrollRect; // ScrollView��ScrollRect���
    private bool isDragItem; // ��ק���Ƿ���������

    void Awake()
    {
        Input.multiTouchEnabled = false; // ���ƶ�ָ��ק
        /*panel = GameObject.Find("/UI/Canvas/GamePanel").GetComponent<Transform>();
        scrollRect = GameObject.Find("/UI/Canvas/GamePanel/Selections/ScrollView").GetComponent<ScrollRect>();
        isDragItem = false;*/
    }

    // Start is called before the first frame update
    void Start()
    {
        ParentWhenDrag = GameObject.Find("UI/Canvas/GamePanel");
        ypController = GameObject.Find("UI").GetComponent<YPController>(); 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentInitial = transform.parent.gameObject;
        transform.GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
        transform.SetParent(ParentWhenDrag.transform);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GameObject obj = eventData.pointerCurrentRaycast.gameObject;
        //Debug.Log(obj.transform.name);
        //Debug.Log(transform.name);
        if(obj.transform.tag == "board" && ypController.Check(transform.name, obj.transform.name))
        {
            //Debug.Log("��ʼƥ��");
            transform.AddComponent<Image>();
            Sprite sprite = transform.GetComponent<Image>().sprite;
            obj.AddComponent<Image>();
            obj.GetComponent<Image>().sprite = sprite;
            obj.GetComponent<Image>().color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
            //Debug.Log("���ƥ��");
            transform.SetParent(ParentInitial.transform);
            int index = int.Parse(transform.name.Remove(0, 5));
            transform.SetSiblingIndex(index);
            transform.gameObject.SetActive(false);
        }
        else
        {
            transform.SetParent(ParentInitial.transform);
            int index = int.Parse(transform.name.Remove(0, 5));
            transform.SetSiblingIndex(index);
        }
        transform.GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
}

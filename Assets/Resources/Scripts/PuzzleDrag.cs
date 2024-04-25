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

    private Vector3 touchOffset; // 拖拽的偏移量
    private Transform panel; // 场景中的Panel，设置拖拽过程中的父物体
    private ScrollRect scrollRect; // ScrollView的ScrollRect组件
    private bool isDragItem; // 拖拽的是否是子物体

    void Awake()
    {
        Input.multiTouchEnabled = false; // 限制多指拖拽
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
            //Debug.Log("开始匹配");
            transform.AddComponent<Image>();
            Sprite sprite = transform.GetComponent<Image>().sprite;
            obj.AddComponent<Image>();
            obj.GetComponent<Image>().sprite = sprite;
            obj.GetComponent<Image>().color = new Color(255.0f, 255.0f, 255.0f, 255.0f);
            //Debug.Log("完成匹配");
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

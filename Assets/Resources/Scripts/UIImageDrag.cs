using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// 实现UI图片拖拽功能
public class UIImageDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("是否精准拖拽")]
    public bool m_isPrecision;

    [Header("是否开启缩放")]
    public bool isCanZoom;

    [Tooltip("最大放大倍数")]
    public float m_maxScale = 4.0f;

    [Tooltip("最小缩小倍数")]
    public float m_minScale = 0.4f;

    // 存储图片中心点与鼠标点击点的偏移量
    private Vector3 m_offset;

    // 存储当前拖拽图片的RectTransform组件
    private RectTransform m_rt;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 如果精准拖拽则进行计算偏移量操作
        if(m_isPrecision)
        {
            // 存储点击时的鼠标坐标
            Vector3 tWorldPos;
            // UI屏幕坐标转换为世界坐标
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rt, eventData.position, eventData.pressEventCamera, out tWorldPos);
            // 计算偏移量
            m_offset = transform.position - tWorldPos;
        }
        // 否则，偏移量默认为0
        else
        {
            m_offset = Vector3.zero;
        }

        SetDraggedPosition(eventData);
        
        // throw new System.NotImplementedException();
    }

    public void OnDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
        // throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        SetDraggedPosition(eventData);
        // throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        // 初始化
        m_rt = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if(isCanZoom)
        {
            float s = Input.GetAxis("Mouse ScrollWheel");
            if(s>0)
            {
                Magnify();
            }
            else if(s<0)
            {
                Shrink();
            }
        }
    }

    void Magnify()
    {
        if(m_rt.localScale.x<m_maxScale)
        {
            m_rt.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    void Shrink()
    {
        if (m_rt.localScale.x > m_minScale)
        {
            m_rt.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    public void ImageReset()
    {
        m_rt.localScale = Vector3.one;
        m_rt.localPosition = Vector3.zero;
    }

    private void SetDraggedPosition(PointerEventData eventData)
    {
        // 存储当前鼠标所在位置
        Vector3 globalMousePos;
        // UI屏幕坐标转换为世界坐标
        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rt,eventData.position,eventData.pressEventCamera,out globalMousePos))
        {
            // 设置位置及偏移量
            m_rt.position = globalMousePos + m_offset;
        }
    }
}

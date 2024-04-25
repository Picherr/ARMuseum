using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ʵ��UIͼƬ��ק����
public class UIImageDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("�Ƿ�׼��ק")]
    public bool m_isPrecision;

    [Header("�Ƿ�������")]
    public bool isCanZoom;

    [Tooltip("���Ŵ���")]
    public float m_maxScale = 4.0f;

    [Tooltip("��С��С����")]
    public float m_minScale = 0.4f;

    // �洢ͼƬ���ĵ�����������ƫ����
    private Vector3 m_offset;

    // �洢��ǰ��קͼƬ��RectTransform���
    private RectTransform m_rt;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // �����׼��ק����м���ƫ��������
        if(m_isPrecision)
        {
            // �洢���ʱ���������
            Vector3 tWorldPos;
            // UI��Ļ����ת��Ϊ��������
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rt, eventData.position, eventData.pressEventCamera, out tWorldPos);
            // ����ƫ����
            m_offset = transform.position - tWorldPos;
        }
        // ����ƫ����Ĭ��Ϊ0
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
        // ��ʼ��
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
        // �洢��ǰ�������λ��
        Vector3 globalMousePos;
        // UI��Ļ����ת��Ϊ��������
        if(RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rt,eventData.position,eventData.pressEventCamera,out globalMousePos))
        {
            // ����λ�ü�ƫ����
            m_rt.position = globalMousePos + m_offset;
        }
    }
}

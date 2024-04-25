using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ScrollView : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    // 获取组件
    public ScrollRect Rect;
    public GameObject Content;
    private float[] posArray = new float[] { 0f, 0.5f, 1.0f };
    private float targetPos;
    // 判断是否正在拖拽
    private bool isDrag = false;
    int index = 0;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDrag = true;
        // throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
        Vector2 pos = Rect.normalizedPosition;
        float x = Mathf.Abs(pos.x - posArray[0]);
        for(int i = 0;i < 3; i++)
        {
            float temp = Mathf.Abs(pos.x - posArray[i]);
            if(temp <= x)
            {
                x = temp;
                index = i;
            }
        }
        targetPos = posArray[index];
      
        // throw new System.NotImplementedException();
    }

    // Start is called before the first frame update
    void Start()
    {
        Rect = GetComponent<ScrollRect>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isDrag)
        {
            Rect.horizontalNormalizedPosition = Mathf.Lerp(Rect.horizontalNormalizedPosition, targetPos, Time.deltaTime * 4);
        }
    }
}

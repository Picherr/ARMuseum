using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MapSizeController : MonoBehaviour
{
    public Vector3 initialSca;
    public static MapSizeController instance;
    IEnumerator ie;
    FingerIE finger_num = FingerIE.zero;

    // 存储当前拖拽图片的RectTransform组件
    private RectTransform m_rt;

    void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_rt = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount == 0)
        {
            if (finger_num != FingerIE.zero)
            {
                StopCoroutine(ie);
                ie = null;
                finger_num = FingerIE.zero;
            }
        }
        else if (Input.touchCount == 2)
        {
            if (finger_num != FingerIE.TwoFinger)
            {
                if (ie != null)
                {
                    StopCoroutine(ie);
                }
                ie = IIMonitorMouseTwoFinger();
                StartCoroutine(ie);
                finger_num = FingerIE.TwoFinger;
            }
        }
    }

    IEnumerator IIMonitorMouseTwoFinger()
    {
        Touch firstOldTouch;
        Touch secondOldTouch;
        Touch firstNewTouch;
        Touch secondNewTouch;
        float oldDistance;
        float newDistance;

        while(true)
        {
            firstOldTouch = Input.GetTouch(0);
            secondOldTouch = Input.GetTouch(1);
            oldDistance = Vector2.Distance(firstOldTouch.position, secondOldTouch.position);
            yield return 0;
            firstNewTouch = Input.GetTouch(0);
            secondNewTouch = Input.GetTouch(1);
            newDistance = Vector2.Distance(firstNewTouch.position, secondNewTouch.position);
            if(oldDistance > newDistance && this.transform.localScale.x > 1.0f)
            {
                this.transform.localScale -= new Vector3(0.01f, 0.01f / 2048 * 1536, 0f);
            }
            else if(oldDistance < newDistance && this.transform.localScale.x < 1.5f)
            {
                this.transform.localScale += new Vector3(0.01f, 0.01f / 2048 * 1536, 0f);
            }
        }
    }
}

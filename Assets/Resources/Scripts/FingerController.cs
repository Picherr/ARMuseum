using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum FingerIE
{
    zero,
    OneFinger,
    TwoFinger,
}

public class FingerController : MonoBehaviour
{
    public Vector3 initialRot;
    public Vector3 initialSca;
    public static FingerController instance;
    IEnumerator ie;
    FingerIE finger_num = FingerIE.zero;

    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount==0)
        {
            if(finger_num!=FingerIE.zero)
            {
                StopCoroutine(ie);
                ie = null;
                finger_num = FingerIE.zero;
            }
        }
        else if(Input.touchCount==1)
        {
            if(finger_num!=FingerIE.OneFinger)
            {
                if(ie!=null)
                {
                    StopCoroutine(ie);
                }
                ie = IMonitorMouseOneFinger();
                StartCoroutine(ie);
                finger_num = FingerIE.OneFinger;
            }
        }
        else if(Input.touchCount==2)
        {
            if(finger_num!=FingerIE.TwoFinger)
            {
                if(ie!=null)
                {
                    StopCoroutine(ie);
                }
                ie = IIMonitorMouseTwoFinger();
                StartCoroutine(ie);
                finger_num = FingerIE.TwoFinger;
            }
        }
    }

    IEnumerator IMonitorMouseOneFinger()
    {
        Touch oneFingerTouch;
        while(true)
        {
            oneFingerTouch = Input.GetTouch(0);
            if(oneFingerTouch.phase==TouchPhase.Moved)
            {
                Vector2 deltaPos = oneFingerTouch.deltaPosition;
                transform.Rotate(-Vector3.up * deltaPos.x * 0.2f, Space.Self);
            }
            yield return 0;
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
            if (oldDistance > newDistance && this.transform.localScale.x > 0.05f)
            {
                this.transform.localScale -= Vector3.one * 0.01f;
            }
            else if (oldDistance < newDistance && this.transform.localScale.x < 1.5f)
            {
                this.transform.localScale += Vector3.one * 0.01f;
            }
        }
    }

    public void ResetRot()
    {
        this.transform.localEulerAngles = initialRot;
        this.transform.localScale = initialSca;
    }
}

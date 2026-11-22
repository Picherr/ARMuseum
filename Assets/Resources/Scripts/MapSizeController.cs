using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARMuseum
{
    public class MapSizeController : MonoBehaviour
    {
        public Vector3 initialSca;
        public static MapSizeController instance;
        private bool isPinching;
        private int firstFingerId;
        private int secondFingerId;
        private float previousDistance;
        private RectTransform m_rt;
        private ScrollRect scrollRect;
        private bool scrollRectSuspended;
        private bool scrollRectWasEnabled;
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerData;
        private EventSystem pointerEventSystem;

        void Awake()
        {
            m_rt = GetComponent<RectTransform>();
            scrollRect = GetComponentInParent<ScrollRect>();
        }

        void OnEnable()
        {
            instance = this;
        }

        void Update()
        {
            ProcessTouches(Input.touches);
        }

        private void ProcessTouches(Touch[] touches)
        {
            if (touches.Length != 2 || !IsActive(touches[0]) || !IsActive(touches[1]))
            {
                EndGesture();
                return;
            }

            var first = touches[0];
            var second = touches[1];
            float distance = Vector2.Distance(first.position, second.position);
            if (!isPinching || firstFingerId != first.fingerId || secondFingerId != second.fingerId)
            {
                EndGesture();
                if (!IsTouchOnMap(first) || !IsTouchOnMap(second))
                {
                    return;
                }
                isPinching = true;
                firstFingerId = first.fingerId;
                secondFingerId = second.fingerId;
                previousDistance = distance;
                if (scrollRect != null)
                {
                    scrollRectWasEnabled = scrollRect.enabled;
                    scrollRectSuspended = true;
                    scrollRect.StopMovement();
                    scrollRect.enabled = false;
                }
                return;
            }

            float step = distance > previousDistance ? 0.01f : distance < previousDistance ? -0.01f : 0f;
            if (step != 0f)
            {
                float scaleChange = Mathf.Clamp(transform.localScale.x + step, 1f, 1.5f) - transform.localScale.x;
                transform.localScale += new Vector3(scaleChange, scaleChange * 1536f / 2048f, 0f);
            }
            previousDistance = distance;
        }

        private bool IsTouchOnMap(Touch touch)
        {
            if (m_rt == null)
            {
                return false;
            }
            var eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                return false;
            }
            if (pointerData == null || pointerEventSystem != eventSystem)
            {
                pointerEventSystem = eventSystem;
                pointerData = new PointerEventData(eventSystem);
            }
            pointerData.Reset();
            pointerData.pointerId = touch.fingerId;
            pointerData.position = touch.position;
            raycastResults.Clear();
            eventSystem.RaycastAll(pointerData, raycastResults);
            foreach (var result in raycastResults)
            {
                if (result.module is GraphicRaycaster)
                {
                    return result.gameObject != null && result.gameObject.transform.IsChildOf(transform);
                }
            }
            return false;
        }

        private static bool IsActive(Touch touch)
        {
            return touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
        }

        private void EndGesture()
        {
            if (scrollRectSuspended)
            {
                scrollRectSuspended = false;
                if (scrollRect != null)
                {
                    scrollRect.enabled = scrollRectWasEnabled;
                }
            }
            isPinching = false;
            firstFingerId = -1;
            secondFingerId = -1;
            previousDistance = 0f;
        }

        private void OnDisable()
        {
            EndGesture();
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                EndGesture();
            }
        }
    }

}

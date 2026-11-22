using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARMuseum
{
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
        private FingerIE finger_num = FingerIE.zero;
        private int firstFingerId;
        private int secondFingerId;
        private float previousDistance;
        private readonly Dictionary<int, bool> uiTouches = new Dictionary<int, bool>();
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerData;
        private EventSystem pointerEventSystem;

        void OnEnable()
        {
            instance = this;
        }

        void Update()
        {
            ProcessTouches(Input.touches);
        }

        // 每帧使用同一份触摸快照，不在协程恢复后访问已经抬起的手指。
        private void ProcessTouches(Touch[] touches)
        {
            foreach (var touch in touches)
            {
                if (!IsActive(touch))
                {
                    uiTouches.Remove(touch.fingerId);
                }
                else if (touch.phase == TouchPhase.Began || !uiTouches.ContainsKey(touch.fingerId))
                {
                    uiTouches[touch.fingerId] = IsOverUI(touch);
                }
            }

            if (touches.Length == 0)
            {
                uiTouches.Clear();
            }
            if (touches.Length < 1 || touches.Length > 2 || !CanUseTouch(touches[0]) ||
                (touches.Length == 2 && !CanUseTouch(touches[1])))
            {
                EndGesture();
                return;
            }

            var first = touches[0];
            if (touches.Length == 1)
            {
                if (finger_num != FingerIE.OneFinger || firstFingerId != first.fingerId)
                {
                    EndGesture();
                    finger_num = FingerIE.OneFinger;
                    firstFingerId = first.fingerId;
                    return;
                }
                if (first.phase == TouchPhase.Moved)
                {
                    transform.Rotate(-Vector3.up * first.deltaPosition.x * 0.2f, Space.Self);
                }
                return;
            }

            var second = touches[1];
            float distance = Vector2.Distance(first.position, second.position);
            if (finger_num != FingerIE.TwoFinger || firstFingerId != first.fingerId ||
                secondFingerId != second.fingerId)
            {
                EndGesture();
                finger_num = FingerIE.TwoFinger;
                firstFingerId = first.fingerId;
                secondFingerId = second.fingerId;
                previousDistance = distance;
                return;
            }

            float step = distance > previousDistance ? 0.01f : distance < previousDistance ? -0.01f : 0f;
            if (step != 0f)
            {
                float scaleChange = Mathf.Clamp(transform.localScale.x + step, 0.05f, 1.5f) - transform.localScale.x;
                transform.localScale += Vector3.one * scaleChange;
            }
            previousDistance = distance;
        }

        private bool CanUseTouch(Touch touch)
        {
            bool isUI;
            return IsActive(touch) && uiTouches.TryGetValue(touch.fingerId, out isUI) && !isUI;
        }

        private static bool IsActive(Touch touch)
        {
            return touch.phase != TouchPhase.Ended && touch.phase != TouchPhase.Canceled;
        }

        private bool IsOverUI(Touch touch)
        {
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
                if (result.module is GraphicRaycaster && BlocksModelGesture(result.gameObject))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool BlocksModelGesture(GameObject target)
        {
            if (target == null)
            {
                return false;
            }
            // 透明的 ScrollRect 子图、按钮和拼图片仍属于 UI；只有无交互的透明装饰允许穿透。
            if (target.GetComponentInParent<Selectable>() != null ||
                ExecuteEvents.GetEventHandler<IPointerDownHandler>(target) != null ||
                ExecuteEvents.GetEventHandler<IPointerClickHandler>(target) != null ||
                ExecuteEvents.GetEventHandler<IDragHandler>(target) != null ||
                ExecuteEvents.GetEventHandler<IScrollHandler>(target) != null)
            {
                return true;
            }
            var graphic = target.GetComponent<Graphic>();
            return graphic == null || graphic.color.a > 0f;
        }

        private void EndGesture()
        {
            finger_num = FingerIE.zero;
            firstFingerId = -1;
            secondFingerId = -1;
            previousDistance = 0f;
        }

        private void OnDisable()
        {
            EndGesture();
            uiTouches.Clear();
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
                uiTouches.Clear();
            }
        }

        public void ResetRot()
        {
            EndGesture();
            this.transform.localEulerAngles = initialRot;
            this.transform.localScale = initialSca;
        }
    }

}

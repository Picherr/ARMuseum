using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace ARMuseum
{
    public class PuzzleDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private GameObject ParentWhenDrag;
        [SerializeField] private YPController ypController;
        private static PuzzleDrag activeDrag;
        private int dragPointerId;
        private Transform initialParent;
        private int initialSiblingIndex;
        private Vector3 initialPosition;
        private Vector3 initialScale;
        private Quaternion initialRotation;
        private CanvasGroup canvasGroup;
        private bool initialBlocksRaycasts;

        void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData == null || !isActiveAndEnabled || activeDrag != null ||
                ParentWhenDrag == null || transform.parent == null || canvasGroup == null)
            {
                return;
            }

            activeDrag = this;
            dragPointerId = eventData.pointerId;
            initialParent = transform.parent;
            initialSiblingIndex = transform.GetSiblingIndex();
            initialPosition = transform.localPosition;
            initialScale = transform.localScale;
            initialRotation = transform.localRotation;
            initialBlocksRaycasts = canvasGroup.blocksRaycasts;
            canvasGroup.blocksRaycasts = false;
            transform.SetParent(ParentWhenDrag.transform, true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null || activeDrag != this || eventData.pointerId != dragPointerId)
            {
                return;
            }

            var dragPlane = ParentWhenDrag != null ? ParentWhenDrag.transform as RectTransform : null;
            Vector3 position;
            if (dragPlane != null && RectTransformUtility.ScreenPointToWorldPointInRectangle(
                dragPlane, eventData.position, eventData.pressEventCamera, out position))
            {
                transform.position = position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData == null || activeDrag != this || eventData.pointerId != dragPointerId)
            {
                return;
            }

            var target = eventData.pointerCurrentRaycast.gameObject;
            var sourceImage = GetComponent<Image>();
            bool matched = target != null && target.CompareTag("board") &&
                ypController != null && sourceImage != null && ypController.Check(name, target.name);
            if (matched)
            {
                var targetImage = target.GetComponent<Image>();
                if (targetImage == null)
                {
                    targetImage = target.AddComponent<Image>();
                }
                targetImage.sprite = sourceImage.sprite;
                targetImage.color = Color.white;
            }

            FinishDrag();
            if (matched)
            {
                gameObject.SetActive(false);
            }
        }

        private void OnDisable()
        {
            FinishDrag();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                FinishDrag();
            }
        }

        private void FinishDrag()
        {
            if (activeDrag != this)
            {
                return;
            }

            activeDrag = null;
            if (initialParent != null)
            {
                transform.SetParent(initialParent, false);
                transform.SetSiblingIndex(initialSiblingIndex);
                transform.localPosition = initialPosition;
                transform.localScale = initialScale;
                transform.localRotation = initialRotation;
            }
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = initialBlocksRaycasts;
            }
        }
    }

}

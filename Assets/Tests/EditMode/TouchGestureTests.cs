using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ARMuseum.Tests.EditMode
{
    public class TouchGestureTests
    {
        private readonly List<Object> objects = new List<Object>();
        private TouchTestEventSystem eventSystem;
        private EventSystem previousEventSystem;
        private TouchTestRaycaster raycaster;
        private bool previousMultiTouch;

        [SetUp]
        public void SetUp()
        {
            previousMultiTouch = Input.multiTouchEnabled;
            previousEventSystem = EventSystem.current;
            eventSystem = Create("EventSystem").AddComponent<TouchTestEventSystem>();
            eventSystem.Register();
            EventSystem.current = eventSystem;
            var canvas = Create("Canvas", typeof(RectTransform), typeof(Canvas));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            raycaster = canvas.AddComponent<TouchTestRaycaster>();
            raycaster.Register();
        }

        [TearDown]
        public void TearDown()
        {
            if (raycaster != null)
            {
                raycaster.Unregister();
            }
            if (eventSystem != null)
            {
                eventSystem.Unregister();
            }
            for (int i = objects.Count - 1; i >= 0; i--)
            {
                if (objects[i] != null)
                {
                    var gameObject = objects[i] as GameObject;
                    var puzzle = gameObject != null ? gameObject.GetComponent<PuzzleDrag>() : null;
                    if (puzzle != null)
                    {
                        Invoke(puzzle, "OnDisable");
                    }
                    Object.DestroyImmediate(objects[i]);
                }
            }
            objects.Clear();
            if (previousEventSystem != null)
            {
                EventSystem.current = previousEventSystem;
            }
            Input.multiTouchEnabled = previousMultiTouch;
        }

        [Test]
        public void PuzzleDrag_UsesOnePointerAndRestoresEmptyDrop()
        {
            Input.multiTouchEnabled = true;
            bool supportedMultiTouchState = Input.multiTouchEnabled;
            var parent = Create("Selections", typeof(RectTransform));
            var overlay = Create("DragOverlay", typeof(RectTransform));
            var first = CreatePuzzle("Image11", parent.transform, overlay);
            var second = CreatePuzzle("Image10", parent.transform, overlay);
            first.transform.localPosition = new Vector3(20f, 30f, 0f);
            int initialIndex = first.transform.GetSiblingIndex();

            Assert.AreEqual(supportedMultiTouchState, Input.multiTouchEnabled, "拼图组件不能修改全局多点触控设置");
            first.OnBeginDrag(Pointer(1));
            second.OnBeginDrag(Pointer(2));
            Assert.AreEqual(overlay.transform, first.transform.parent);
            Assert.AreEqual(parent.transform, second.transform.parent);
            Assert.IsTrue(second.GetComponent<CanvasGroup>().blocksRaycasts);

            first.OnEndDrag(Pointer(2));
            Assert.AreEqual(overlay.transform, first.transform.parent, "其他手指不能结束当前拖拽");
            first.OnEndDrag(Pointer(1));
            Assert.AreEqual(parent.transform, first.transform.parent);
            Assert.AreEqual(initialIndex, first.transform.GetSiblingIndex());
            Assert.AreEqual(new Vector3(20f, 30f, 0f), first.transform.localPosition);
            Assert.IsTrue(first.GetComponent<CanvasGroup>().blocksRaycasts);
            Assert.IsTrue(first.gameObject.activeSelf);

            second.OnBeginDrag(Pointer(2));
            Assert.AreEqual(overlay.transform, second.transform.parent, "空白落点后应释放拖拽占用");
        }

        [Test]
        public void PuzzleDrag_DisableRestoresStateAndReleasesPointer()
        {
            var parent = Create("Selections", typeof(RectTransform));
            var overlay = Create("DragOverlay", typeof(RectTransform));
            var first = CreatePuzzle("Image11", parent.transform, overlay);
            var second = CreatePuzzle("Image10", parent.transform, overlay);
            first.OnBeginDrag(Pointer(1));
            Invoke(first, "OnDisable");
            Assert.AreEqual(parent.transform, first.transform.parent);
            Assert.IsTrue(first.GetComponent<CanvasGroup>().blocksRaycasts);
            second.OnBeginDrag(Pointer(2));
            Assert.AreEqual(overlay.transform, second.transform.parent);
        }

        [Test]
        public void PuzzleDrag_MatchingDropReusesImagesAndHidesPiece()
        {
            var parent = Create("Selections", typeof(RectTransform));
            var overlay = Create("DragOverlay", typeof(RectTransform));
            var puzzle = CreatePuzzle("Image11", parent.transform, overlay);
            var controller = Create("PuzzleController").AddComponent<YPController>();
            var table = (System.Collections.IDictionary)GetField(controller, "table");
            if (table.Count == 0)
            {
                Invoke(controller, "Awake");
            }
            SetField(puzzle, "ypController", controller);
            var target = Create("Part1_Image0", typeof(RectTransform), typeof(Image));
            target.tag = "board";
            var sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), Vector2.zero);
            objects.Add(sprite);
            puzzle.GetComponent<Image>().sprite = sprite;
            var pointer = Pointer(1);
            pointer.pointerCurrentRaycast = new RaycastResult { gameObject = target };

            puzzle.OnBeginDrag(pointer);
            puzzle.OnEndDrag(pointer);

            Assert.AreEqual(1, puzzle.GetComponents<Image>().Length);
            Assert.AreEqual(1, target.GetComponents<Image>().Length);
            Assert.AreSame(sprite, target.GetComponent<Image>().sprite);
            Assert.AreEqual(Color.white, target.GetComponent<Image>().color);
            Assert.IsFalse(puzzle.gameObject.activeSelf);
            Assert.AreEqual(parent.transform, puzzle.transform.parent);
            Assert.IsTrue(puzzle.GetComponent<CanvasGroup>().blocksRaycasts);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void ModelPinch_InterruptedTouchCountStartsNewBaseline(int touchCount)
        {
            var model = Create("Model").AddComponent<FingerController>();
            Process(model, TouchAt(1, -200f), TouchAt(2, -100f));
            Process(model, TouchAt(1, -210f), TouchAt(2, -90f));
            Assert.That(model.transform.localScale.x, Is.EqualTo(1.01f).Within(0.0001f));

            Process(model, Interruption(touchCount, -500f));
            Process(model, TouchAt(1, -500f), TouchAt(2, -100f));
            Assert.That(model.transform.localScale.x, Is.EqualTo(1.01f).Within(0.0001f));
            Process(model, TouchAt(1, -510f), TouchAt(2, -90f));
            Assert.That(model.transform.localScale.x, Is.EqualTo(1.02f).Within(0.0001f));
        }

        [Test]
        public void ModelGesture_TouchBeginningOnUIStaysOwnedByUI()
        {
            raycaster.Target = Create("PuzzleOrMapUI");
            var model = Create("Model").AddComponent<FingerController>();
            Process(model, TouchAt(1, 10f, TouchPhase.Began));
            Process(model, TouchAt(1, -100f, TouchPhase.Moved, 100f));
            Assert.AreEqual(Quaternion.identity, model.transform.localRotation);
            Process(model, TouchAt(1, -100f), TouchAt(2, -200f));
            Process(model, TouchAt(1, -90f), TouchAt(2, -210f));
            Assert.AreEqual(Vector3.one, model.transform.localScale);

            Process(model);
            Process(model, TouchAt(1, -100f, TouchPhase.Began));
            Process(model, TouchAt(1, -90f, TouchPhase.Moved, 10f));
            Assert.AreNotEqual(Quaternion.identity, model.transform.localRotation);
        }

        [TestCase(0.01f)]
        [TestCase(2f)]
        public void ModelPinch_StationaryFingersDoNotChangeExistingScale(float initialScale)
        {
            var model = Create("Model").AddComponent<FingerController>();
            model.transform.localScale = Vector3.one * initialScale;
            Process(model, TouchAt(1, -200f), TouchAt(2, -100f));
            Process(model, TouchAt(1, -200f), TouchAt(2, -100f));

            Assert.AreEqual(Vector3.one * initialScale, model.transform.localScale);
        }

        [Test]
        public void ModelGesture_PassiveTransparentImageDoesNotBlockARView()
        {
            var decoration = Create("TransparentWireFrame", typeof(RectTransform), typeof(Image));
            decoration.GetComponent<Image>().color = Color.clear;
            raycaster.Target = decoration;
            var model = Create("Model").AddComponent<FingerController>();

            Process(model, TouchAt(1, 10f, TouchPhase.Began));
            Process(model, TouchAt(1, 20f, TouchPhase.Moved, 10f));

            Assert.AreNotEqual(Quaternion.identity, model.transform.localRotation);
        }

        [TestCase(typeof(ScrollRect))]
        [TestCase(typeof(Button))]
        [TestCase(typeof(PuzzleDrag))]
        public void ModelGesture_TransparentInteractiveUIStillBlocksModel(System.Type componentType)
        {
            var control = Create("InteractiveUI", typeof(RectTransform), componentType);
            var image = Create("TransparentChild", typeof(RectTransform), typeof(Image));
            image.transform.SetParent(control.transform, false);
            image.GetComponent<Image>().color = Color.clear;
            raycaster.Target = image;
            var model = Create("Model").AddComponent<FingerController>();

            Process(model, TouchAt(1, 10f, TouchPhase.Began));
            Process(model, TouchAt(1, 20f, TouchPhase.Moved, 10f));

            Assert.AreEqual(Quaternion.identity, model.transform.localRotation);
            Process(model, TouchAt(1, 10f), TouchAt(2, 110f));
            Process(model, TouchAt(1, 5f), TouchAt(2, 115f));
            Assert.AreEqual(Vector3.one, model.transform.localScale);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        public void MapPinch_InterruptedTouchCountStartsNewBaseline(int touchCount)
        {
            var map = Create("Map", typeof(RectTransform)).AddComponent<MapSizeController>();
            Invoke(map, "Awake");
            raycaster.Target = map.gameObject;
            Process(map, TouchAt(1, 10f), TouchAt(2, 110f));
            Process(map, TouchAt(1, 5f), TouchAt(2, 115f));
            Assert.That(map.transform.localScale.x, Is.EqualTo(1.01f).Within(0.0001f));

            Process(map, Interruption(touchCount, 10f));
            Process(map, TouchAt(1, 10f), TouchAt(2, 410f));
            Assert.That(map.transform.localScale.x, Is.EqualTo(1.01f).Within(0.0001f));
            Process(map, TouchAt(1, 5f), TouchAt(2, 415f));
            Assert.That(map.transform.localScale.x, Is.EqualTo(1.02f).Within(0.0001f));
        }

        [Test]
        public void Pinch_DisableAndCanceledTouchDoNotReuseOldDistance()
        {
            var model = Create("Model").AddComponent<FingerController>();
            Process(model, TouchAt(1, -200f), TouchAt(2, -100f));
            Invoke(model, "OnDisable");
            Process(model, TouchAt(1, -500f), TouchAt(2, -100f));
            Assert.AreEqual(Vector3.one, model.transform.localScale);
            Process(model, TouchAt(1, -510f, TouchPhase.Canceled), TouchAt(2, -90f));
            Assert.AreEqual(Vector3.one, model.transform.localScale);

            var map = Create("Map", typeof(RectTransform)).AddComponent<MapSizeController>();
            Invoke(map, "Awake");
            raycaster.Target = map.gameObject;
            Process(map, TouchAt(1, 10f), TouchAt(2, 110f));
            Invoke(map, "OnDisable");
            Process(map, TouchAt(1, 10f), TouchAt(2, 410f));
            Assert.AreEqual(Vector3.one, map.transform.localScale);
            Process(map, TouchAt(1, 5f, TouchPhase.Canceled), TouchAt(2, 415f));
            Assert.AreEqual(Vector3.one, map.transform.localScale);
        }

        [Test]
        public void MapPinch_DoesNotReactToOtherUI()
        {
            var map = Create("Map", typeof(RectTransform)).AddComponent<MapSizeController>();
            Invoke(map, "Awake");
            raycaster.Target = Create("OtherPanel");
            Process(map, TouchAt(1, 10f), TouchAt(2, 110f));
            Process(map, TouchAt(1, 5f), TouchAt(2, 115f));
            Assert.AreEqual(Vector3.one, map.transform.localScale);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MapPinch_PausesScrollAndRestoresItsPreviousState(bool wasEnabled)
        {
            var scrollRect = Create("MapScroll", typeof(RectTransform)).AddComponent<ScrollRect>();
            var map = Create("Map", typeof(RectTransform)).AddComponent<MapSizeController>();
            map.transform.SetParent(scrollRect.transform, false);
            scrollRect.content = (RectTransform)map.transform;
            scrollRect.enabled = wasEnabled;
            scrollRect.velocity = new Vector2(100f, 100f);
            Invoke(map, "Awake");
            raycaster.Target = map.gameObject;

            Process(map, TouchAt(1, 10f), TouchAt(2, 110f));
            Assert.IsFalse(scrollRect.enabled, "缩放期间不能由 ScrollRect 同时拖动地图");
            Assert.AreEqual(Vector2.zero, scrollRect.velocity);
            Process(map, TouchAt(1, 10f));
            Assert.AreEqual(wasEnabled, scrollRect.enabled);

            Process(map, TouchAt(1, 10f), TouchAt(2, 110f));
            Invoke(map, "OnDisable");
            Assert.AreEqual(wasEnabled, scrollRect.enabled, "地图关闭时也必须恢复 ScrollRect 状态");
        }

        private GameObject Create(string name, params System.Type[] components)
        {
            var result = new GameObject(name, components);
            objects.Add(result);
            return result;
        }

        private PuzzleDrag CreatePuzzle(string name, Transform parent, GameObject overlay)
        {
            var result = Create(name, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            result.transform.SetParent(parent, false);
            var puzzle = result.AddComponent<PuzzleDrag>();
            SetField(puzzle, "ParentWhenDrag", overlay);
            Invoke(puzzle, "Awake");
            return puzzle;
        }

        private PointerEventData Pointer(int pointerId)
        {
            return new PointerEventData(eventSystem) { pointerId = pointerId, position = Vector2.zero };
        }

        private static Touch TouchAt(int fingerId, float x, TouchPhase phase = TouchPhase.Moved, float deltaX = 0f)
        {
            return new Touch { fingerId = fingerId, position = new Vector2(x, 0f),
                phase = phase, deltaPosition = new Vector2(deltaX, 0f) };
        }

        private static Touch[] Interruption(int count, float x)
        {
            var touches = new Touch[count];
            for (int i = 0; i < count; i++)
            {
                touches[i] = TouchAt(i + 1, x + i * 10f);
            }
            return touches;
        }

        private static void Process(MonoBehaviour controller, params Touch[] touches)
        {
            Invoke(controller, "ProcessTouches", (object)touches);
        }

        private static void Invoke(object instance, string method, params object[] arguments)
        {
            instance.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(instance, arguments);
        }

        private static object GetField(object instance, string field)
        {
            return instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(instance);
        }

        private static void SetField(object instance, string field, object value)
        {
            instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(instance, value);
        }
    }

    // EditMode 不保证自动调用 OnEnable，显式注册并防止生命周期重复注册。
    public class TouchTestEventSystem : EventSystem
    {
        private bool registered;

        public void Register()
        {
            OnEnable();
        }

        public void Unregister()
        {
            OnDisable();
        }

        protected override void OnEnable()
        {
            if (!registered)
            {
                base.OnEnable();
                registered = true;
            }
        }

        protected override void OnDisable()
        {
            if (registered)
            {
                registered = false;
                base.OnDisable();
            }
        }
    }

    // 提供确定的 UI 命中，避免依赖测试窗口尺寸或真实摄像头/触摸硬件。
    public class TouchTestRaycaster : GraphicRaycaster
    {
        public GameObject Target;
        private bool registered;

        public void Register()
        {
            OnEnable();
        }

        public void Unregister()
        {
            OnDisable();
        }

        protected override void OnEnable()
        {
            if (!registered)
            {
                base.OnEnable();
                registered = true;
            }
        }

        protected override void OnDisable()
        {
            if (registered)
            {
                registered = false;
                base.OnDisable();
            }
        }

        public override void Raycast(PointerEventData eventData, List<RaycastResult> resultAppendList)
        {
            if (Target != null && eventData.position.x >= 0f)
            {
                resultAppendList.Add(new RaycastResult { gameObject = Target, module = this });
            }
        }
    }
}

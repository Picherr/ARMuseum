using UnityEngine;

public class SlidingDirection : MonoBehaviour
{
    enum SlideVector
    {
        None,
        Up,
        Down,
        Left,
        Right
    }

    [SerializeField]
    private bool executeMultipleTimes = false; // 是否支持多次执行
    [SerializeField]
    private float offsetTime = 0.1f; // 判断的时间间隔
    [SerializeField]
    private float slidingDistance = 80f; // 滑动的最小距离

    private Vector2 touchFirst = Vector2.zero; // 手指开始按下的位置

    private Vector2 touchSecond = Vector2.zero; // 手指拖动的位置

    private SlideVector currentVector = SlideVector.None; // 当前滑动方向

    private float timer; // 时间计数器

    void OnGUI()
    {
        if(Event.current.type==EventType.MouseDown)
        {
            touchFirst = Event.current.mousePosition;
        }

        if(Event.current.type==EventType.MouseDrag)
        {
            touchSecond = Event.current.mousePosition;

            timer += Time.deltaTime;

            if(timer>offsetTime)
            {
                touchSecond = Event.current.mousePosition;
                Vector2 slideDirection = touchFirst - touchSecond;
                float x = slideDirection.x;
                float y = slideDirection.y;

                if(y + slidingDistance < x && y> -x - slidingDistance)
                {
                    if(!executeMultipleTimes && currentVector == SlideVector.Left)
                    {
                        return;
                    }

                    Debug.Log("Left");

                    currentVector = SlideVector.Left;
                }
                else if(y > x + slidingDistance && y < -x - slidingDistance)
                {
                    if (!executeMultipleTimes && currentVector == SlideVector.Right)
                    {
                        return;
                    }

                    Debug.Log("right");

                    currentVector = SlideVector.Right;
                }
                else if(y > x + slidingDistance && y - slidingDistance > -x)
                {
                    if(!executeMultipleTimes && currentVector == SlideVector.Up)
                    {
                        return;
                    }

                    Debug.Log("up");

                    currentVector = SlideVector.Up;
                }
                else if(y + slidingDistance < x && y < -x - slidingDistance)
                {
                    if(!executeMultipleTimes && currentVector == SlideVector.Down)
                    {
                        return;
                    }

                    Debug.Log("down");

                    currentVector = SlideVector.Down;
                }

                timer = 0;
                touchFirst = touchSecond;
            }
        }

        if(Event.current.type==EventType.MouseUp)
        {
            currentVector = SlideVector.None; // 初始化方向
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

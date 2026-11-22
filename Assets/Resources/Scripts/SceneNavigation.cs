using UnityEngine;

namespace ARMuseum
{
    /// <summary>
    /// 场景跳转入口。承接原 GameController 中的 changeToXXX 系列方法，
    /// 供场景/预制体按钮的 onClick 序列化引用调用。
    /// </summary>
    public class SceneNavigation : MonoBehaviour
    {
        public void changeToBeginning()
        {
            SceneLoader.Load(SceneNames.Beginning);
        }

        public void changeToNYWD()
        {
            SceneLoader.Load(SceneNames.NanYueWenDi);
        }

        public void changeToMYDG()
        {
            SceneLoader.Load(SceneNames.MeiYuDaGuan);
        }

        public void changeToHLYF()
        {
            SceneLoader.Load(SceneNames.HaiLuYangFan);
        }

        public void changeToSHQJ()
        {
            SceneLoader.Load(SceneNames.ShengHuoQiJu);
        }

        public void changeToNavigation()
        {
            SceneLoader.Load(SceneNames.Navigation);
        }
    }
}

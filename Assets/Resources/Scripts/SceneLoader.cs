using UnityEngine.SceneManagement;

namespace ARMuseum
{
    /// <summary>
    /// 场景加载的统一入口。
    /// 所有场景跳转都经由这里，后续可在此统一加入异步加载、过渡动画或埋点。
    /// </summary>
    public static class SceneLoader
    {
        public static void Load(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }

}

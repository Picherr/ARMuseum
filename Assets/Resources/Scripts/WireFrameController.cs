using UnityEngine;

namespace ARMuseum
{
    /// <summary>
    /// 线框图（WireFrame）展示控制。承接原 GameController 中的 NYWDUpdate / HLYFUpdate 逻辑，
    /// 根据识别图 imagetarget 的激活状态切换 IntroImage1 的显示。
    /// </summary>
    public class WireFrameController : MonoBehaviour
    {
        public GameObject[] imagetarget;
        public GameObject WireFramePanel;

        private void Update()
        {
            if (imagetarget == null || imagetarget.Length == 0)
                return;

            string path = "/UI/Canvas/WireFramePanel/WireFrame/IntroImage1";
            if (imagetarget[0].activeSelf)
            {
                WireFramePanel.SetActive(true);
                GameObject.Find(path).SetActive(true);
            }
            else if (!imagetarget[0].activeSelf && GameObject.Find(path).activeSelf)
            {
                GameObject.Find(path).SetActive(false);
                WireFramePanel.SetActive(false);
            }
        }
    }
}

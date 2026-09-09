using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARMuseum
{
    /// <summary>
    /// 美玉大观（MeiYuDaGuan）场景的游戏面板与拼图提示控制。
    /// 承接原 GameController 中的 MYDGUpdate / GameChoice / GameOver 逻辑。
    /// </summary>
    public class GamePanelController : MonoBehaviour
    {
        public GameObject[] imagetarget;
        public GameObject WireFramePanel;

        private GameObject ypWarning;
        private Animator animator;
        private GameObject GamePanel;

        private int index = -1;

        private void Start()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.MeiYuDaGuan)
                return;

            ypWarning = GameObject.Find("/UI/Canvas/YPWarning");
            animator = GameObject.Find("/UI/Canvas/GamePanel/Selections").GetComponent<Animator>();
            GamePanel = GameObject.Find("/UI/Canvas/GamePanel");
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.MeiYuDaGuan)
                return;

            for (int i = 1; i <= imagetarget.Length; i++)
            {
                string path = "UI/Canvas/WireFramePanel/WireFrame/IntroImage" + i;
                if (i < 3)
                {
                    if (imagetarget[i - 1].activeSelf)
                    {
                        WireFramePanel.SetActive(true);
                        GameObject.Find(path).SetActive(true);
                    }
                    else if (!imagetarget[i - 1].activeSelf && GameObject.Find(path).activeSelf)
                    {
                        GameObject.Find(path).SetActive(false);
                        WireFramePanel.SetActive(false);
                    }
                }
                else
                {
                    if (imagetarget[i - 1].activeSelf)
                    {
                        index = i - 2;
                        ypWarning.SetActive(true);
                    }
                }
            }
        }

        public void GameChoice()
        {
            string choice = "/UI/Canvas/GamePanel/Background/Part" + index;
            GamePanel.SetActive(true);
            GameObject.Find(choice).SetActive(true);
            animator.SetBool("isPlay", true);
            animator.SetFloat("Speed", 1.0f);
        }

        public void GameOver()
        {
            string choice = "/UI/Canvas/GamePanel/Background/Part" + index;
            GameObject.Find(choice).SetActive(false);
            GamePanel.SetActive(false);
            animator.SetFloat("Speed", -1.0f);
            animator.SetBool("isPlay", false);
            index = 1;
        }
    }
}

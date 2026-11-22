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

        [SerializeField] private GameObject ypWarning;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject[] introImages; // IntroImage1 / IntroImage2
        [SerializeField] private GameObject[] partPanels;  // Part1 .. PartN

        private int index = -1;

        private void Update()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.MeiYuDaGuan)
                return;

            for (int i = 1; i <= imagetarget.Length; i++)
            {
                if (i < 3)
                {
                    var introImage = introImages[i - 1];
                    if (imagetarget[i - 1].activeSelf)
                    {
                        WireFramePanel.SetActive(true);
                        introImage.SetActive(true);
                    }
                    else if (!imagetarget[i - 1].activeSelf && introImage.activeSelf)
                    {
                        introImage.SetActive(false);
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
            gamePanel.SetActive(true);
            partPanels[index - 1].SetActive(true);
            animator.SetBool("isPlay", true);
            animator.SetFloat("Speed", 1.0f);
        }

        public void GameOver()
        {
            partPanels[index - 1].SetActive(false);
            gamePanel.SetActive(false);
            animator.SetFloat("Speed", -1.0f);
            animator.SetBool("isPlay", false);
            index = 1;
        }
    }
}

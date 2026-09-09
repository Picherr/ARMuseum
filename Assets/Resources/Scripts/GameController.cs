using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public GameObject TipImg;
    private Button AudioBtn;
    public GameObject AudioOn;
    public GameObject AudioOff;
    public GameObject TouchTip;

    private GameObject ypWarning;
    private Animator animator;
    private GameObject GamePanel;

    public GameObject[] imagetarget;
    public GameObject WireFramePanel;

    private int index = -1;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name == SceneNames.Navigation)
            return;
        AudioBtn = GameObject.Find("/UI/MenuUI/Canvas/ButtonGroup/Audio").GetComponent<Button>();
        AudioBtn.onClick.AddListener(Audio);
        if(SceneManager.GetActiveScene().name == SceneNames.MeiYuDaGuan)
        {
            ypWarning = GameObject.Find("/UI/Canvas/YPWarning");
            animator = GameObject.Find("/UI/Canvas/GamePanel/Selections").GetComponent<Animator>();
            GamePanel = GameObject.Find("/UI/Canvas/GamePanel");
        }
    }

    private void Update()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        switch(sceneName)
        {
            case SceneNames.NanYueWenDi:
                NYWDUpdate();
                break;
            case SceneNames.MeiYuDaGuan:
                MYDGUpdate();
                break;
            case SceneNames.HaiLuYangFan:
                HLYFUpdate();
                break;
            default:
                break;
        }
    }

    public void Tip()
    {
        TipImg.SetActive(true);
        Invoke("changeToMM", 5.0f);
    }

    private void changeToMM()
    {
        changeToNYWD();
    }

    public void changeToBeginning()
    {
        SceneManager.LoadScene(SceneNames.Beginning);
    }

    public void changeToNYWD()
    {
        SceneManager.LoadScene(SceneNames.NanYueWenDi);
    }

    public void changeToMYDG()
    {
        SceneManager.LoadScene(SceneNames.MeiYuDaGuan);
    }

    public void changeToHLYF()
    {
        SceneManager.LoadScene(SceneNames.HaiLuYangFan);
    }

    public void changeToSHQJ()
    {
        SceneManager.LoadScene(SceneNames.ShengHuoQiJu);
    }

    public void changeToGTYY()
    {
        SceneManager.LoadScene("GongTingYanLe");
    }

    public void changeToBQCM()
    {
        SceneManager.LoadScene("BingQiCheMa");
    }

    public void changeToNavigation()
    {
        SceneManager.LoadScene(SceneNames.Navigation);
    }

    private void NYWDUpdate()
    {
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

    private void MYDGUpdate()
    {
        for(int i = 1; i <= imagetarget.Length; i++)
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

    private void HLYFUpdate()
    {
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

    private void Audio()
    {
        string Path1;
        string Path2;
        Sprite sprite1;
        Sprite sprite2;

        if (Variables.isAudioOn) // 当前声音功能开启
        {
            Path1 = "UI/Button/BClick/静音";
            Path2 = "UI/Button/AClick/静音";
            AudioOff.SetActive(true);
        }
        else
        {
            Path1 = "UI/Button/BClick/语音";
            Path2 = "UI/Button/AClick/语音";
            AudioOn.SetActive(true);
        }
        sprite1 = Resources.Load<Sprite>(Path1) as Sprite;
        AudioBtn.image.sprite = sprite1;
        sprite2 = Resources.Load<Sprite>(Path2) as Sprite;
        SpriteState spriteState = new SpriteState();
        spriteState.pressedSprite = sprite2;
        AudioBtn.spriteState = spriteState;

        Variables.isAudioOn = !Variables.isAudioOn;
        Invoke("AudioTurnOff", 1.0f);
    }

    private void AudioTurnOff()
    {
        AudioOn.SetActive(false);
        AudioOff.SetActive(false);
    }

    public void TouchTipOpen()
    {
        TouchTip.SetActive(true);
        Invoke("TouchTipOff", 2.0f);
    }

    private void TouchTipOff()
    {
        TouchTip.SetActive(false);
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

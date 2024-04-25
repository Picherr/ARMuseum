using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

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
        if (SceneManager.GetActiveScene().name == "Navigation")
            return;
        AudioBtn = GameObject.Find("/UI/MenuUI/Canvas/ButtonGroup/Audio").GetComponent<Button>();
        AudioBtn.onClick.AddListener(Audio);
        if(SceneManager.GetActiveScene().name == "MeiYuDaGuan")
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
            case "NanYueWenDi":
                NYWDUpdate();
                break;
            case "MeiYuDaGuan":
                MYDGUpdate();
                break;
            case "HaiLuYangFan":
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
        SceneManager.LoadScene("Beginning");
    }

    public void changeToNYWD()
    {
        SceneManager.LoadScene("NanYueWenDi");
    }

    public void changeToMYDG()
    {
        SceneManager.LoadScene("MeiYuDaGuan");
    }

    public void changeToHLYF()
    {
        SceneManager.LoadScene("HaiLuYangFan");
    }

    public void changeToSHQJ()
    {
        SceneManager.LoadScene("ShengHuoQiJu");
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
        SceneManager.LoadScene("Navigation");
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

        if (Variables.isAudioOn) // µ±Ç°ÉùÒô¹¦ÄÜ¿ªÆô
        {
            Path1 = "UI/Button/BClick/¾²Òô";
            Path2 = "UI/Button/AClick/¾²Òô";
            AudioOff.SetActive(true);
        }
        else
        {
            Path1 = "UI/Button/BClick/ÓïÒô";
            Path2 = "UI/Button/AClick/ÓïÒô";
            AudioOn.SetActive(true);
        }
        sprite1 = Resources.Load<Sprite>(Path1) as Sprite;
        Instantiate(sprite1);
        AudioBtn.image.sprite = sprite1;
        sprite2 = Resources.Load<Sprite>(Path2) as Sprite;
        Instantiate(sprite2);
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

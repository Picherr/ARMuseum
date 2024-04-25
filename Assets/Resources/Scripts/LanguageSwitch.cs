using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSwitch : MonoBehaviour
{
    public Button IntroBtn;
    public Button LanSwitchBtn;
    public Button MapBtn;
    public Button NavigBtn;

    public Image Intro1;
    public Image Intro2;
    public Image Intro3;

    public Image Details;

    public Button Floor1Btn;
    public Button Floor2Btn;
    public Button SHQJBtn;
    public Button BQCMBtn;
    public Button MYDGBtn;
    public Button HLYFBtn;
    public Button GTYYBtn;
    public Button NYWDBtn;

    // Start is called before the first frame update
    void Start()
    {
        // 初始化
        if (Variables.isChinese) // 此时是中文
            setAll("cn");
        else // 此时是英文
            setAll("en");
    }

    public void Change()
    {
        Variables.isChinese = !Variables.isChinese;
        if (!Variables.isChinese) // 换成英文
        {
            setAll("en");
        }
        else // 换成英文
        {
            setAll("cn");
        }
    }

    private void setAll(string Lan)
    {
        setMenuBtn(Lan);
        setNavigationBtn(Lan);
        setIntroduction(Lan);
        setDetails(Lan);
    }

    private void setMenuBtn(string Lan)
    {
        Set(Lan, "list", "Intro", IntroBtn);
        Set(Lan, "list", "LanSwitch", LanSwitchBtn);
        Set(Lan, "list", "Map", MapBtn);
        Set(Lan, "list", "Navig", NavigBtn);

    }

    private void setNavigationBtn(string Lan)
    {
        Set(Lan, "listNavigation", "Floor1", Floor1Btn);
        Set(Lan, "listNavigation", "Floor2", Floor2Btn);

        Set(Lan, "listNavigation", "BQCM", BQCMBtn);
        Set(Lan, "listNavigation", "GTYY", GTYYBtn);
        Set(Lan, "listNavigation", "HLYF", HLYFBtn);
        Set(Lan, "listNavigation", "MYDG", MYDGBtn);
        Set(Lan, "listNavigation", "NYWD", NYWDBtn);
        Set(Lan, "listNavigation", "SHQJ", SHQJBtn);
    }

    private void setIntroduction(string Lan)
    {
        Set(Lan, "Introduction", "Intro1", Intro1);
        Set(Lan, "Introduction", "Intro2", Intro2);
        Set(Lan, "Introduction", "Intro3", Intro3);
    }

    private void setDetails(string Lan)
    {
        if (Details == null)
            return;
        string target = Details.sprite.name;
        target = target.Substring(0, target.Length - 3);
        Set(Lan, "Details", target, Details);
    }

    private void Set(string Lan, string Directory, string Target, Button obj)
    {
        string Path = "UI/" + Directory + "/" + Lan + "/" + Target + "_" + Lan;
        Sprite sprite = Resources.Load<Sprite>(Path) as Sprite;
        Instantiate(sprite);

        //string objPath = Target + "Btn";
        //Button btn = GameObject.Find(objPath).GetComponent<Button>();
        obj.image.sprite = sprite;
    }

    private void Set(string Lan, string Directory, string Target, Image obj)
    {
        string Path = "UI/" + Directory + "/" + Lan + "/" + Target + "_" + Lan;
        Debug.Log(Path);
        Sprite sprite = Resources.Load<Sprite>(Path) as Sprite;
        Instantiate(sprite);

        obj.sprite = sprite;
    }
}

using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NavigationController : MonoBehaviour
{
    public Button F1btn;
    public Button F2btn;
    public Button Navigbtn;
    public Button Backbtn;
    public GameObject Floor1;
    public GameObject Floor2;

    public GameObject Panel;

    private Animator f1Anim;
    private Animator f2Anim;

    private bool isOn1;
    private bool isOn2;
    private bool isHide1;
    private bool isHide2;

    // Start is called before the first frame update
    void Start()
    {
        f1Anim = Floor1.GetComponent<Animator>();
        f2Anim = Floor2.GetComponent<Animator>();

        isOn1 = true;
        isOn2 = true;
        isHide1 = false;
        isHide2 = false;
        
        F1btn.onClick.AddListener(HideFloor1);
        F2btn.onClick.AddListener(HideFloor2);
        Navigbtn.onClick.AddListener(Open);
        Backbtn.onClick.AddListener(Close);

        f1Anim.SetBool("isOn", true);
        f2Anim.SetBool("isOn", true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HideFloor1()
    {
        if(isOn1)
        {
            if (!isHide1)
            {
                f1Anim.SetBool("isHide", true);
                f1Anim.SetFloat("SpeedCtrl", 1);
                isHide1 = true;
            }
            else
            {
                f1Anim.SetBool("isHide", false);
                f1Anim.SetFloat("SpeedCtrl", -1);
                isHide1 = false;
            }
        }
    }

    public void HideFloor2()
    {
        if(isOn2)
        {
            if (!isHide2)
            {
                f2Anim.SetBool("isHide", true);
                f2Anim.SetFloat("SpeedCtrl", 1);
                isHide2 = true;
            }
            else
            {
                f2Anim.SetBool("isHide", false);
                f2Anim.SetFloat("SpeedCtrl", -1);
                isHide2 = false;
            }
        }
    }

    public void Open()
    {
        isOn1 = true;
        isOn2 = true;
        isHide1 = false;
        isHide2 = false;
        Panel.SetActive(true);
    }

    public void Close()
    {
        CloseFloor1();
        CloseFloor2();
        Invoke("ClosePanel", 0.2f);
    }

    public void CloseFloor1()
    {
        if (isHide1) // 此时UI处于隐藏状态
        {
            f1Anim.SetBool("isHide", true);
        }
        else
        {
            f1Anim.SetFloat("SpeedCtrl", -1);
        }
        f1Anim.SetBool("isOn", false);
    }

    public void CloseFloor2()
    {
        if (isHide2) // 此时UI处于隐藏状态
        {
            f2Anim.SetBool("isHide", true);
        }
        else
        {
            f2Anim.SetFloat("SpeedCtrl", -1);
        }
        f2Anim.SetBool("isOn", false);
    }

    public void ClosePanel()
    {
        bool isPanelValid = Panel.activeSelf;
        if (isPanelValid) // 此时NavigPanel处于active
        {
            Panel.SetActive(false);
        }
        else
        {
            return;
        }
    }
}

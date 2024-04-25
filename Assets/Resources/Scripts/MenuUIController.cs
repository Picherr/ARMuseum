using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuUIController : MonoBehaviour
{
    public GameObject SettingMenu;
    public GameObject Game;
    public GameObject Details;
    public GameObject Sound;
    public GameObject Panel;

    private bool isPanelActive = false;

    public void OpenSettingMenu()
    {
        if(!isPanelActive) // 未点击列表菜单
        {
            Game.SetActive(false);
            Details.SetActive(false);
            Sound.SetActive(false);
            Panel.SetActive(true);

            isPanelActive = true;
        }
        else
        {
            Game.SetActive(true);
            Details.SetActive(true);
            Sound.SetActive(true);
            Panel.SetActive(false);

            isPanelActive = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

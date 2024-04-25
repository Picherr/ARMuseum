using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class NavInfoController : MonoBehaviour
{
    public Sprite[] sprites;
    public Image img;

    public void NavInfo(string Target)
    {
        for(int i = 0; i < 6; i++)
        {
            string name = sprites[i].name.Substring(0, sprites[i].name.Length - 3);
            if (name == Target)
            {
                string Path;
                if(Variables.isChinese)
                {
                    Path = "UI/NavigationInfo/cn/" + Target + "_cn";
                }
                else
                {
                    Path = "UI/NavigationInfo/en/" + Target + "_en";
                }
                Debug.Log(Path);
                Sprite sprite = Resources.Load<Sprite>(Path) as Sprite;
                Instantiate(sprite);
                sprites[i] = sprite;
                img.sprite = sprites[i];
            }
        }
    }
}

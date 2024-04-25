using easyar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YPController : MonoBehaviour
{
    Hashtable table = new Hashtable();

    private void Awake()
    {
        // Part1
        table.Add("Image11", "Part1_Image0");
        table.Add("Image10", "Part1_Image1");
        table.Add("Image4", "Part1_Image2");
        table.Add("Image1", "Part1_Image3");
        table.Add("Image12", "Part1_Image4");

        // Part2
        table.Add("Image16", "Part2_Image0");
        table.Add("Image2", "Part2_Image1");
        table.Add("Image15", "Part2_Image2");
        table.Add("Image17", "Part2_Image3");

        // Part3
        table.Add("Image6", "Part3_Image0");
        table.Add("Image5", "Part3_Image1");
        table.Add("Image14", "Part3_Image2");
        table.Add("Image7", "Part3_Image3");
        table.Add("Image9", "Part3_Image4");

        // Part4
        table.Add("Image18", "Part4_Image0");
        table.Add("Image3", "Part4_Image1");
        table.Add("Image0", "Part4_Image2");
        table.Add("Image13", "Part4_Image3");
        table.Add("Image8", "Part4_Image4");
    }

    public bool Check(string dragName, string dripName)
    {
        if (table[dragName].ToString() == dripName)
        {
            Debug.Log("∆•≈‰≥…π¶");
            return true;
        }
        else
        {
            Debug.Log("∆•≈‰ ß∞‹");
            return false;
        }
    }
}

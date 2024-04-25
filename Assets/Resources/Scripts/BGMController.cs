using System.Collections;
using UnityEngine;

public class BGMController : MonoBehaviour
{
    private GameObject bgmPrefab;
    private GameObject bgmInstance = null;

    // Start is called before the first frame update
    void Start()
    {
        bgmInstance = GameObject.FindGameObjectWithTag("sound");
        if(bgmInstance == null)
        {
            bgmInstance = (GameObject)Instantiate(bgmPrefab);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

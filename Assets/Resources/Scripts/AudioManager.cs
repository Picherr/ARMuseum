using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource audioS;
    private GameController gameController;

    // Start is called before the first frame update
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        gameController = GetComponent<GameController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AudioPlay(AudioClip clip)
    {
        Debug.Log(clip.name);
        audioS.PlayOneShot(clip);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationSfxManager : MonoBehaviour
{
    public GameObject sendMessageTarget;
    public AudioClip[] a;
    GameManager gm;
    AudioSource audio;

    // Start is called before the first frame update
    void Awake()
    {
        gm = FindObjectOfType<GameManager>();
    }

    public void PlaySFX(int index)
    {
        if (audio == null)
            audio = GetComponent<AudioSource>();

        if(audio != null)
            audio.PlayOneShot(a[index]);
    }

    public void SendMessageToTarget(string s)
    {
        sendMessageTarget.SendMessage(s);
    }
}

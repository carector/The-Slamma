using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorUtility : MonoBehaviour
{
    public GameObject sendMessageTarget;
    public AudioClip[] sfx;

    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendMessageToTarget(string message)
    {
        sendMessageTarget.SendMessage(message, SendMessageOptions.DontRequireReceiver);
    }

    public void SelfDestruct()
    {
        Destroy(this.gameObject);
    }
}

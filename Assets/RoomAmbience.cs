using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomAmbience : MonoBehaviour
{
    RoomManager room;
    GameManager gm;
    AudioSource audio;

    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        audio.volume = 0;
        room = GetComponentInParent<RoomManager>();
        gm = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        int value = 0;
        if (room == gm.currentRoom)
            value = 1;
        audio.volume = Mathf.Lerp(audio.volume, value, 0.25f);
    }
}

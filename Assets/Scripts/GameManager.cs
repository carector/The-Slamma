using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Transform cam;
    Image[] hearts;
    Image[] doors;
    PlayerController ply;

    public int score;
    public Transform globalCameraReference;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
        hearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            hearts[i] = GameObject.Find("Heart" + i).GetComponent<Image>();
        }

        doors = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            doors[i] = GameObject.Find("Door" + i).GetComponent<Image>();
            doors[i].color = Color.clear;
        }
        Cursor.lockState = CursorLockMode.Locked;
        cam = GameObject.Find("CameraAnimations").transform;

        //Time.timeScale = 0;
    }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // Update health
        for (int i = 0; i < ply.health; i++)
            hearts[i].color = Color.white;
        for (int i = ply.health; i < 3; i++)
            hearts[i].color = Color.black;

        if(ply.doorCharges == 0)
        {
            for(int i = 0; i < 3; i++)
                doors[i].color = Color.clear;
        }
        else
        {
            for (int i = 0; i < ply.doorCharges; i++)
                doors[i].color = Color.white;
            for (int i = ply.doorCharges; i < 3; i++)
                doors[i].color = Color.black;
        }
    } 

    public void ScreenShake()
    {
        StartCoroutine(ScreenShakeCoroutine(5));
    }
    public void ScreenShake(float intensity)
    {
        StartCoroutine(ScreenShakeCoroutine(intensity));
    }

    IEnumerator ScreenShakeCoroutine(float intensity)
    {
        for (int i = 0; i < 10; i++)
        {
            cam.localPosition = new Vector2(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f)) * intensity;
            intensity /= 1.25f;
            yield return new WaitForFixedUpdate();
        }
        cam.localPosition = Vector2.zero;
    }
}

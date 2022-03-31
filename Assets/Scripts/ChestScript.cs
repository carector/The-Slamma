using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChestScript : MonoBehaviour
{
    GameManager gm;
    PlayerController ply;
    Animator anim;
    Image heartImage;
    TextMeshProUGUI pointsText;

    bool isHeart;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        pointsText = transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>();
        heartImage = transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Image>();

        heartImage.color = Color.clear;
        pointsText.color = Color.clear;
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenChest()
    {
        anim.Play("ChestOpen");
    }

    public void ShowReward()
    {
        float rand = Random.Range(0, 1f);
        rand *= Mathf.Clamp(3 - ply.health, 1, 2);

        if (rand >= 0.66f)
            isHeart = true;

        if (isHeart)
            heartImage.color = Color.white;
        else
            pointsText.color = Color.white;
    }

    public void ReceiveReward()
    {
        if (isHeart && ply.health > 0)
            ply.health = Mathf.Clamp(ply.health + 1, 0, 3);
        else
            gm.score += 1000;
            
    }
}

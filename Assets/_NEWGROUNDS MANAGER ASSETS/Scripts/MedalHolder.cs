using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MedalHolder : MonoBehaviour
{
    Image icon;
    TextMeshProUGUI nameText;
    TextMeshProUGUI descText;
    Image pointsImage;
    NewgroundsManager ng;

    // Start is called before the first frame update
    void Start()
    {
        ng = FindObjectOfType<NewgroundsManager>();
        icon = transform.GetChild(0).GetComponent<Image>();
        nameText = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        descText = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        pointsImage = transform.GetChild(3).GetComponent<Image>();
    }

    public void SetDisplayedValues(NewgroundsManager.MedalData md)
    {
        SetDisplayedValues(ng.GetSpriteFromMedalID(md.id), md.name, md.description, md.points, md.unlocked, md.isSecret);
    }

    public void SetDisplayedValues(Sprite _icon, string _name, string _description, int _points, bool unlocked, bool secret)
    {
        if (unlocked)
            icon.sprite = _icon;
        else
            icon.sprite = ng.lockedIcon;

        if (secret && !unlocked)
        {
            nameText.text = "Secret medal";
            descText.text = "????????????";
            pointsImage.color = Color.clear;
        }
        else
        {
            nameText.text = _name;
            descText.text = _description;
            pointsImage.sprite = ng.GetSpriteFromScoreValue(_points);
            if (pointsImage.sprite == null)
                pointsImage.color = Color.clear;
            else
                pointsImage.color = Color.white;
        }
        
    }
}

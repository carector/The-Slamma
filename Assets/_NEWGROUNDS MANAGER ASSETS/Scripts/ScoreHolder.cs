using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreHolder : MonoBehaviour
{
    TextMeshProUGUI username;
    TextMeshProUGUI value;
    TextMeshProUGUI rank;

    // Start is called before the first frame update
    void Start()
    {
        username = transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        value = transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        rank = transform.GetChild(3).GetComponent<TextMeshProUGUI>();
    }

    public void SetDisplayedValues(NewgroundsManager.ScoreData sd)
    {
        SetDisplayedValues(sd.username, sd.value, sd.rank, sd.userId, sd.useTimeFormat);
    }

    public void SetDisplayedValues(string username, int value, int rank, int id, bool useTimeFormat)
    {
        this.username.text = username;
        if (useTimeFormat)
            this.value.text = ConvertToNiceTime(value / 1000f);
        else
            this.value.text = value.ToString();

        this.rank.text = rank.ToString();

        if (NGIO.hasUser && id == NGIO.user.id)
            this.username.color = Color.green;
        else
            this.username.color = Color.white;
    }

    string ConvertToNiceTime(float timer)
    {
        int minutes = Mathf.FloorToInt(timer / 60F);
        int seconds = Mathf.FloorToInt(timer - minutes * 60);
        int milliseconds = Mathf.FloorToInt(((timer - (minutes * 60) - seconds)) * 100);
        string niceTime = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
        return niceTime;
    }

    public void SetVisibility(bool state)
    {
        this.gameObject.SetActive(state);
    }
}

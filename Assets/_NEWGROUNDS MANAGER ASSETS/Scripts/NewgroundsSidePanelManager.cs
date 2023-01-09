using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class NewgroundsSidePanelManager : MonoBehaviour
{
    private MedalHolder[] medalHolders;
    private ScoreHolder[] scoreHolders;
    private Animator anim;
    private NewgroundsManager ng;
    private Animator loadingIcon;
    private RectTransform medalsPanel;
    private RectTransform scoresPanel;
    private TextMeshProUGUI errorText;
    private TextMeshProUGUI pageNumberText;
    private TextMeshProUGUI usernameText;
    private TMP_Dropdown scoresDropdown;
    private bool isSidePanelOpen;

    EventSystem ev;
    bool loadingResults;

    void Awake()
    {
        Transform medalParent = GameObject.Find("NewgroundsVertViewport").transform;
        Transform scoresParent = GameObject.Find("NewgroundsScoresPanel").transform;
        medalHolders = new MedalHolder[8];
        for (int i = 0; i < medalHolders.Length; i++)
            medalHolders[i] = medalParent.GetChild(i).GetComponent<MedalHolder>();

        scoreHolders = new ScoreHolder[7];
        for(int i = 0; i < 7; i++)
            scoreHolders[i] = scoresParent.GetChild(i).GetComponent<ScoreHolder>(); 

        anim = GetComponent<Animator>();
        ng = FindObjectOfType<NewgroundsManager>();
        ev = FindObjectOfType<EventSystem>();
        errorText = GameObject.Find("NewgroundsErrorText").GetComponent<TextMeshProUGUI>();
        errorText.text = "";

        pageNumberText = GameObject.Find("PageNumberText").GetComponent<TextMeshProUGUI>();
        usernameText = GameObject.Find("NewgroundsUsername").GetComponent<TextMeshProUGUI>();

        loadingIcon = GameObject.Find("NewgroundsMountainLoadingIcon").GetComponent<Animator>();
        medalsPanel = GameObject.Find("NewgroundsMedalsPanel").GetComponent<RectTransform>();
        scoresPanel = GameObject.Find("NewgroundsScoresPanel").GetComponent<RectTransform>();
        scoresDropdown = GameObject.Find("NewgroundsScoresDropdown").GetComponent<TMP_Dropdown>();
    }

    void HideOverlayPanels()
    {
        errorText.text = "";
        loadingIcon.Play("MountainLoadingIcon_Spin", 0, 0);
        medalsPanel.anchoredPosition = new Vector2(1000, medalsPanel.anchoredPosition.y);
        scoresPanel.anchoredPosition = new Vector2(1000, scoresPanel.anchoredPosition.y);
    }

    public void SetDisplayedUsername(string name)
    {
        usernameText.text = name;
    }

    public void OpenPanel()
    {
        isSidePanelOpen = true;
        ng.PlayNewgroundsSFX(2);
        
        anim.Play("NewgroundsSidePanel_SlideIn", 0, 0);

        if(NGIO.hasUser)
            LoadMedalList(false);
    }

    public bool IsSidePanelOpen()
    {
        return isSidePanelOpen;
    }

    public void LoadMedalList(bool playSound)
    {
        HideOverlayPanels();

        if (playSound)
            ng.PlayNewgroundsSFX(3);

        ev.SetSelectedGameObject(null);
        StartCoroutine(LoadMedalListCoroutine());
    }

    public void LoadScoreboardFromDropdownValue()
    {
        LoadScoreboard(scoresDropdown.value);
    }

    void LoadScoreboard(int scoreboardIndex)
    {
        // Don't request scores again if we're already loading them
        if (ng.IsGettingScores())
            return;

        HideOverlayPanels();

        ev.SetSelectedGameObject(null);
        ng.PlayNewgroundsSFX(3);
        StartCoroutine(LoadScoresListCoroutine(ng.scoreboardIds[scoreboardIndex]));
    }

    public void IncreaseScoresPage()
    {
        if (ng.scoresPage == (ng.scores.Count - 1) / 7)
            return;

        ng.PlayNewgroundsSFX(3);
        ng.scoresPage = Mathf.Clamp(ng.scoresPage + 1, 0, Mathf.Min((ng.scores.Count - 1) / 7, 99));
        DisplayScores();
    }

    public void DecreaseScoresPage()
    {
        if (ng.scoresPage == 0)
            return;

        ng.PlayNewgroundsSFX(3);
        ng.scoresPage = Mathf.Clamp(ng.scoresPage - 1, 0, 99);
        DisplayScores();
    }

    public void GoToFirstScoresPage()
    {
        if (ng.scoresPage == 0)
            return;

        ng.PlayNewgroundsSFX(3);
        ng.scoresPage = 0;
        DisplayScores();
    }

    public void GoToLastScoresPage()
    {
        if (ng.scoresPage == (ng.scores.Count - 1) / 7)
            return;

        ng.PlayNewgroundsSFX(3);
        ng.scoresPage = (ng.scores.Count-1) / 7;
        DisplayScores();
    }

    void DisplayScores()
    {
        int max = Mathf.Min(7, ng.scores.Count - ng.scoresPage * 7);
        print(max);
        for (int i = 0; i < max; i++)
        {
            scoreHolders[i].SetVisibility(true);
            scoreHolders[i].SetDisplayedValues(ng.scores[ng.scoresPage * 7 + i]);
        }
        for (int i = max; i < 7; i++)
            scoreHolders[i].SetVisibility(false);

        pageNumberText.text = (ng.scoresPage + 1).ToString();
        scoresPanel.anchoredPosition = new Vector2(0, scoresPanel.anchoredPosition.y);
    }

    IEnumerator LoadMedalListCoroutine()
    {
        yield return ng.LoadMedalsCoroutine();

        // Check for any network errors
        if (!CheckForLoginErrors())
            yield break;

        for (int i = 0; i < ng.medals.Count; i++)
        {
            medalHolders[i].gameObject.SetActive(true);
            medalHolders[i].SetDisplayedValues(ng.medals[i]);
        }
        for (int j = ng.medals.Count; j < medalHolders.Length; j++)
            medalHolders[j].gameObject.SetActive(false);
        medalsPanel.anchoredPosition = new Vector2(0, medalsPanel.anchoredPosition.y);
        loadingIcon.Play("MountainLoadingIcon_Invisible", 0, 0);
    }

    IEnumerator LoadScoresListCoroutine(int scoreboard)
    {
        // Had to modify NGIO.cs to change the limit from 10 scores (default) to 1000 scores
        yield return ng.LoadScoresCoroutine(scoreboard);
        loadingIcon.Play("MountainLoadingIcon_Invisible", 0, 0);

        if (ng.scores.Count == 0)
        {
            errorText.text = "No scores found.";
        }
        else
        {
            DisplayScores();
        }

    }

    bool CheckForLoginErrors()
    {
        print(NGIO.lastConnectionStatus);
        string t = "";
        if(NGIO.lastConnectionStatus == "server-unavailable")
            t = "Error: Could not connect to the Newgrounds server!";
        if (NGIO.lastConnectionStatus == "user-logged-out")
            t = "Error: You are not logged in!";

        errorText.text = t;
        return t == "";
    }

    public void ClosePanel()
    {
        isSidePanelOpen = false;
        ev.SetSelectedGameObject(null);
        ng.PlayNewgroundsSFX(1);
        anim.Play("NewgroundsSidePanel_SlideOut", 0, 0);
    }
}

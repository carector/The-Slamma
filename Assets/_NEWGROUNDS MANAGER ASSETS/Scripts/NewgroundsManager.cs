using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using NewgroundsIO;
using UnityEngine.SceneManagement;

// Heavily adapted from https://github.com/PsychoGoldfishNG/NewgroundsIO-Unity/wiki
public class NewgroundsManager : MonoBehaviour
{
    public string appID;
    public string AESKey;

    [Tooltip("An optional version number to compare to the version set on your API Tools page on Newgrounds\nUse X.X.X format, or leave blank if you aren't using this feature")]
    public string Version = "";

    [Header("Preloading options (disable any you don't use)")]
    [Tooltip("Check to enable debug mode. Disable before publishing your game!")]
    public bool DebugMode = false;
    [Tooltip("Will preload all your medals, and note if the player has them unlocked yet")]
    public bool PreloadMedals = false;
    [Tooltip("Will preload all your scoreboards")]
    public bool PreloadScoreBoards = false;
    [Tooltip("Will preload the user's save slot information")]
    public bool PreloadSaveSlots = false;
    [Space(12)]
    [Tooltip("Will automatically check to see if hosting website is allowed to host this game")]
    public bool AutoCheckHostLicense = false;
    [Tooltip("Will automatically log a new view of this game")]
    public bool AutoLogView = false;

    public int[] scoreboardIds;
    public Sprite[] medalIcons;
    public Sprite[] pointValueIcons;
    public Sprite lockedIcon;
    public RectTransform[] loginPanels;
    public List<MedalData> medals;
    public List<ScoreData> scores;
    public int scoresPage = 0;
    public AudioClip[] sounds;
    [HideInInspector]
    public bool isConnected;

    private NewgroundsSidePanelManager sidePanel;
    private AudioSource audio;
    private Animator medalDisplayerAnimator;
    private Animator loadingIconAnimator;
    private TextMeshProUGUI medalName;
    private TextMeshProUGUI medalValue;
    private TextMeshProUGUI errorText; // For login prompt and listing errors
    private Image medalIcon;
    private IEnumerator cancelCheckCoroutine;

    List<NewgroundsIO.objects.Medal> medalQueue; // If medal unlock popup is already being displayed, wait until it finishes to show the next one
    bool displayingMedalPopup;
    bool gettingScores;

    int activeScreen = -1;

    public struct MedalData
    {
        public int id;
        public string name;
        public string description;
        public int points;
        public bool unlocked;
        public bool isSecret;

        public MedalData(int _id, string _name, string _desc, int _points, bool _unlocked, bool _secret)
        {
            id = _id;
            name = _name;
            description = _desc;
            points = _points;
            unlocked = _unlocked;
            isSecret = _secret;
        }
    }

    public struct ScoreData
    {
        public int userId;
        public string username;
        public int value;
        public int rank;
        public bool useTimeFormat;

        public ScoreData(int _userId, string _username, int _value, int _rank, bool _useTimeFormat)
        {
            userId = _userId;
            username = _username;
            value = _value;
            rank = _rank;
            useTimeFormat = _useTimeFormat;
        }
    }

    // Start is called before the first frame update
    void Awake()
    {
        medalQueue = new List<NewgroundsIO.objects.Medal>();
        medals = new List<MedalData>();
        scores = new List<ScoreData>();

        audio = GetComponent<AudioSource>();
        errorText = GameObject.Find("NewgroundsErrorText").GetComponent<TextMeshProUGUI>();
        medalDisplayerAnimator = GameObject.Find("MedalBackground").GetComponent<Animator>();
        medalIcon = medalDisplayerAnimator.transform.GetChild(0).GetComponent<Image>();
        medalName = medalDisplayerAnimator.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        medalValue = medalDisplayerAnimator.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        loadingIconAnimator = GameObject.Find("NewgroundsMountainLoadingIcon").GetComponent<Animator>();
        sidePanel = GetComponentInChildren<NewgroundsSidePanelManager>();
        InitializeNGIO();
    }

    public void InitializeNGIO()
    {
        // Don't try to connect if we don't have an internet connection
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            print("Unreachable");
            errorText.text = "Can't connect to the internet";
            return;
        }

        // Bypass host license check if we're on windows
        bool isWindows = Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;

        // Set up the options for NGIO
        var options = new Dictionary<string, object>()
        {
            { "version",            Version },
            { "debugMode",          DebugMode },
            { "checkHostLicense",   AutoCheckHostLicense && !isWindows},
            { "autoLogNewView",     AutoLogView },
            { "preloadMedals",      PreloadMedals },
            { "preloadScoreBoards", PreloadScoreBoards },
            { "preloadSaveSlots",   PreloadSaveSlots },
        };

        //NGIO.LogOut();

        NGIO.Init(appID, AESKey, options);

    }

    void Update()
    {
        if (!NGIO.isInitialized)
            return;

        /** 
         * Even though we call this on every frame, it will only trigger OnConnectionStatusChanged
         * when there is an actual status change
         **/
        StartCoroutine(NGIO.GetConnectionStatus(OnConnectionStatusChanged));
        StartCoroutine(NGIO.KeepSessionAlive());
    }

    /// <summary>
    /// Hide the login button & text, and open the login page in a new browser tab.
    /// This will change the session status to "waiting-for-user"
    /// </summary>
    public void OnLoginButtonClick()
    {
        loadingIconAnimator.Play("MountainLoadingIcon_Spin", 0, 0);
        DisplayLoginPanel(1);
        NGIO.OpenLoginPage();
    }

    /// <summary>
    /// Hide the login button & text, and open the login page in a new browser tab.
    /// This will change the session status to "waiting-for-user"
    /// </summary>
    public void OnDontLoginButtonClick()
    {
        DisplayLoginPanel(-1);
        NGIO.SkipLogin();
    }

    public void OnCancelLoginButtonClick()
    {
        loadingIconAnimator.Play("MountainLoadingIcon_Invisible", 0, 0);
        DisplayLoginPanel(0);
        NGIO.CancelLogin();
    }

    public bool SaveDataExists(int slot)
    {
        if (NGIO.GetSaveSlot(slot) == null)
            return false;
        return NGIO.GetSaveSlot(slot).hasData;
    }

    public void DeleteSaveSlot(int slot)
    {
        StartCoroutine(NGIO.SetSaveSlotData(slot, null));
    }

    public void WriteSaveSlot(int slot, string json)
    {
        StartCoroutine(NGIO.SetSaveSlotData(slot, json, OnSaveDataComplete));
    }

    public void OnSaveDataComplete(NewgroundsIO.objects.SaveSlot slot)
    {
        print("Successfully wrote to save slot " + slot.id);
    }

    public void ReadSaveSlot(int slot)
    {
        StartCoroutine(NGIO.GetSaveSlotData(slot, OnSaveDataLoaded));
    }

    public void OnSaveDataLoaded(string data)
    {
        //gm.DeserializeAndLoadSaveData(data);
    }

    public IEnumerator LoadMedalsCoroutine()
    {
        if (!isConnected)
            yield break;

        NGIO.ngioCore.QueueComponent(new NewgroundsIO.components.Medal.getList());
        if (NGIO.ngioCore.hasQueue) yield return NGIO.ngioCore.ExecuteQueue();

        medals = new List<MedalData>();
        foreach (KeyValuePair<int, NewgroundsIO.objects.Medal> md in NGIO.medals)
        {
            NewgroundsIO.objects.Medal m = md.Value;
            medals.Add(new MedalData(m.id, m.name, m.description, m.value, m.unlocked, m.secret));
        }
    }

    public bool IsGettingScores()
    {
        return gettingScores;
    }

    public IEnumerator LoadScoresCoroutine(int id)
    {
        if (!isConnected)
            yield break;

        this.scores = new List<ScoreData>();
        gettingScores = true;
        yield return NGIO.GetScores(id, "A", "", false, OnScoresLoaded);
        gettingScores = false;
    }

    public void OnScoresLoaded(NewgroundsIO.objects.ScoreBoard board, List<NewgroundsIO.objects.Score> scores, string period, string tag, bool social)
    {
        // hard-coded
        bool useTimeFormat = board.id == 11686;
        int rank = 1;
        int userRank = 0;
        this.scores = new List<ScoreData>();
        scores.ForEach(score =>
        {
            this.scores.Add(new ScoreData(score.user.id, score.user.name, score.value, rank, useTimeFormat));
            if (score.user.id == NGIO.user.id)
                userRank = rank;
            rank++;
        });

        // Find which page the user's ID is on
        scoresPage = userRank / 7;
    }

    void DisplayLoginPanel(int index)
    {
        // -1: Hide all panels
        // 0: Prompt login panel
        // 1: "Waiting for login" panel

        for (int i = 0; i < loginPanels.Length; i++)
        {
            if (i == index)
                loginPanels[i].gameObject.SetActive(true);
            else
                loginPanels[i].gameObject.SetActive(false);
        }

        activeScreen = index;
    }

    public void OnConnectionStatusChanged(string status)
    {
        // You blocked the website hosting this game!
        if (!NGIO.legalHost)
        {
            Debug.LogError("Illegal host detected");
            SceneManager.LoadScene("illegalHostScene");
        }

        // This copy of the game is out of date
        /*if (NGIO.isDeprecated)
        {
            Debug.LogError("Depracated version detected");
        }*/

        // If the user is currently logging in, this will be true.
        if (NGIO.loginPageOpen)
        {
            Debug.LogError("Login page open detected");
            // Here is where we check the actual status of the session.
        }
        else
        {
            switch (status)
            {
                case NGIO.STATUS_CHECKING_LOCAL_VERSION:
                    loadingIconAnimator.Play("MountainLoadingIcon_Spin", 0, 0);
                    break;

                case NGIO.STATUS_PRELOADING_ITEMS:
                    loadingIconAnimator.Play("MountainLoadingIcon_Spin", 0, 0);
                    break;

                case NGIO.STATUS_LOCAL_VERSION_CHECKED:
                    if (cancelCheckCoroutine == null)
                    {
                        cancelCheckCoroutine = DelayBeforeCancellingLogin();
                        StartCoroutine(cancelCheckCoroutine);
                    }
                    break;

                case NGIO.STATUS_LOGIN_REQUIRED:
                    if (cancelCheckCoroutine != null) StopCoroutine(cancelCheckCoroutine);
                    loadingIconAnimator.Play("MountainLoadingIcon_Invisible", 0, 0);
                    Debug.LogError("Login required");
                    DisplayLoginPanel(0);
                    break;

                case NGIO.STATUS_READY:
                    if (cancelCheckCoroutine != null) StopCoroutine(cancelCheckCoroutine);
                    loadingIconAnimator.Play("MountainLoadingIcon_Invisible", 0, 0);

                    if (NGIO.hasUser)
                    {
                        isConnected = true;
                        sidePanel.SetDisplayedUsername(NGIO.user.name);
                        GameObject.Find("NewgroundsLoadingPanel").SetActive(false);
                    }
                    else
                    {
                        Debug.LogError("User DID NOT sign in");
                    }

                    /**
                     * You can close any 'please wait' messages now!
                     */

                    DisplayLoginPanel(-1);
                    //gm.LoadTitleScreenForFirstTime();
                    break;
            }
        }
    }

    IEnumerator DelayBeforeCancellingLogin()
    {
        // Used to escape from cases where newgrounds is unavailable
        yield return new WaitForSeconds(3);
        loadingIconAnimator.Play("MountainLoadingIcon_Invisible", 0, 0);
        errorText.text = "Could not connect to Newgrounds";
        DisplayLoginPanel(-1);
        //gm.LoadTitleScreenForFirstTime();
        StopAllCoroutines();
    }


    public void UnlockMedal(int id)
    {
        if (NGIO.hasUser && !NGIO.GetMedal(id).unlocked)
            StartCoroutine(NGIO.UnlockMedal(id, MedalUnlockedCallback));
    }

    public void PostScore(int id, int value)
    {
        if (NGIO.hasUser)
            StartCoroutine(NGIO.PostScore(id, value, null));
    }

    #region Plumbing
    void MedalUnlockedCallback(NewgroundsIO.objects.Medal medal)
    {
        if (displayingMedalPopup)
            medalQueue.Add(medal);
        else
            StartCoroutine(DisplayMedalCoroutine(medal));
    }

    IEnumerator SetIconFromURL(string url)
    {
        WWW www = new WWW(url);
        yield return www;
        //medalIcon.texture = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
    }

    public Sprite GetSpriteFromMedalID(int id)
    {
        // Would much rather not have to do this but Unity can't read webp files
        // Varies per project
        switch (id)
        {
            case 68301:
                return medalIcons[0];
            case 68302:
                return medalIcons[1];
            case 68303:
                return medalIcons[2];
            case 68304:
                return medalIcons[3];
        }

        return null;
    }

    public Sprite GetSpriteFromScoreValue(int value)
    {
        switch (value)
        {
            case 10:
                return pointValueIcons[0];
                break;
            case 25:
                return pointValueIcons[1];
                break;
            case 50:
                return pointValueIcons[2];
                break;
            case 100:
                return pointValueIcons[3];
                break;
        }
        return null;
    }

    public void PlayNewgroundsSFX(int index)
    {
        audio.PlayOneShot(sounds[index]);
    }

    IEnumerator DisplayMedalCoroutine(NewgroundsIO.objects.Medal medal)
    {
        PlayNewgroundsSFX(0);

        displayingMedalPopup = true;
        medalName.text = medal.name;
        medalValue.text = medal.value + "pts";
        medalIcon.sprite = GetSpriteFromMedalID(medal.id);
        //yield return SetIconFromURL(md.icon);
        medalDisplayerAnimator.Play("MedalDisplayer_SlideIn");
        yield return new WaitForSecondsRealtime(4f);
        medalDisplayerAnimator.Play("MedalDisplayer_SlideOut");
        yield return new WaitForSecondsRealtime(0.65f);

        if (medalQueue.Count > 0)
        {
            StartCoroutine(DisplayMedalCoroutine(medalQueue[0]));
            medalQueue.RemoveAt(0);
        }
        else
            displayingMedalPopup = false;
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    Camera camObject;
    Transform cam;
    Image[] hearts;
    Image[] doors;
    Slider scoreSlider;

    TextMeshProUGUI timerText;
    TextMeshProUGUI scoreText;

    TextMeshProUGUI finalScore;
    TextMeshProUGUI finalTime;
    TextMeshProUGUI finalRooms;
    TextMeshProUGUI scoreThresholdText;
    TextMeshProUGUI scoreMultiplierText;
    TextMeshProUGUI attackDescriptionText;

    GameObject hud;
    RectTransform pauseScreen;
    RectTransform hudScreen;
    RectTransform resultsScreen;
    MedalScoreboardUtility ng;
    PlayerController ply;

    public AudioMixer mixer;
    public int score;
    public int scoreMultiplier = 1;
    public int scoreThreshold = 2500;
    public float displayedScore = 0;
    public int roomsCleared;
    public float runtime;
    public bool timerRunning;
    public bool gamePaused;

    int currentComboPoints; // Points since the score timer was reset

    bool initialized;
    bool loadingLevel;
    bool showingResults;
    bool shownThreshold;

    public Transform globalCameraReference;
    public GameObject doorPrefab;
    public GameObject cellDoorPrefab;
    public GameObject prisonerPrefab;
    public GameObject copPrefab;
    public GameObject exitSignPrefab;
    public GameObject navLinkPrefab;

    public GameObject titleScreenRoom;
    public GameObject tutorialRoomPrefab;
    public GameObject mainRoomPrefab;

    public List<RoomManager> roomPrefabPool;
    public List<RoomManager> hardToReachRoomPrefabPool; // Accessed from doors you need to slam your way into, also appear at lower rates if you're just holding a door
    public RoomManager endRoom;
    public RoomManager currentRoom;

    List<RoomManager> spawnedRooms;
    List<RoomManager> visitedRooms;
    NavigationBaker navBaker;
    AudioSource musicSource;
    Slider sfxSlider;
    Slider musicSlider;
    Slider ambSlider;
    public Animator fadeAnimator;
    RoomManager exitRoom;

    IEnumerator currentMultiplierCountdown = null;

    // Start is called before the first frame update
    void Start()
    {
        navBaker = GetComponent<NavigationBaker>();
        ng = FindObjectOfType<MedalScoreboardUtility>();

        hud = GameObject.Find("HUD");
        sfxSlider = GameObject.Find("SFXSlider").GetComponent<Slider>();
        ambSlider = GameObject.Find("AmbienceSlider").GetComponent<Slider>();
        musicSlider = GameObject.Find("MusicSlider").GetComponent<Slider>();

        timerText = GameObject.Find("RoomNumberText").GetComponent<TextMeshProUGUI>();
        scoreText = GameObject.Find("PointsText").GetComponent<TextMeshProUGUI>();

        hudScreen = GameObject.Find("MainScreen").GetComponent<RectTransform>();
        resultsScreen = GameObject.Find("ResultsScreen").GetComponent<RectTransform>();
        pauseScreen = GameObject.Find("PauseScreen").GetComponent<RectTransform>();

        attackDescriptionText = GameObject.Find("PointsDescriptionText").GetComponent<TextMeshProUGUI>();
        scoreMultiplierText = GameObject.Find("PointsMultiplierText").GetComponent<TextMeshProUGUI>();
        attackDescriptionText.text = "";
        scoreMultiplierText.text = "1x";

        finalScore = GameObject.Find("FinalScore").GetComponent<TextMeshProUGUI>();
        finalTime = GameObject.Find("FinalTime").GetComponent<TextMeshProUGUI>();
        finalRooms = GameObject.Find("FinalRooms").GetComponent<TextMeshProUGUI>();
        scoreThresholdText = GameObject.Find("ThresholdTip").GetComponent<TextMeshProUGUI>();

        if (GameObject.Find("Music") != null)
            musicSource = GameObject.Find("Music").GetComponent<AudioSource>();

        visitedRooms = new List<RoomManager>();
        spawnedRooms = new List<RoomManager>();
        scoreSlider = GameObject.Find("ScoreSlider").GetComponent<Slider>();
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

        cam = GameObject.Find("CameraAnimations").transform;
        camObject = GameObject.Find("Camera").GetComponent<Camera>();

        BakeNavigation();
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator ShowThresholdText()
    {
        scoreThresholdText.rectTransform.anchoredPosition = Vector2.zero;
        yield return new WaitForSeconds(5);
        scoreThresholdText.rectTransform.anchoredPosition = Vector2.down * 1000;
    }

    public void InitializePlayer(bool isTutorial)
    {
        if (isTutorial)
            PlayerPrefs.SetInt("SLAMMA_HAS_PLAYED_TUTORIAL", 1);

        if (isTutorial)
            Instantiate(tutorialRoomPrefab);
        else
            Instantiate(mainRoomPrefab);

        Cursor.lockState = CursorLockMode.Locked;
        hud.SetActive(true);
        LoadVolumeFromPlayerPrefs();
        fadeAnimator.Play("BlackFadeIn", 0, 0);
        Destroy(titleScreenRoom.gameObject);
        ply.states.canMove = true;
        initialized = true;
    }

    public void ShowResultsScreen()
    {
        SetPausedState(false);
        showingResults = true;
        Cursor.lockState = CursorLockMode.None;
        timerRunning = false;
        finalScore.text = scoreText.text;
        finalTime.text = timerText.text;
        finalRooms.text = roomsCleared.ToString();

        resultsScreen.anchoredPosition = Vector2.zero;
        hudScreen.anchoredPosition = Vector2.up * 2000;

        ng.PostScore(11685, score);
        ng.PostScore(11686, Mathf.RoundToInt(runtime * 1000));

        ng.UnlockMedal(68301);
        if (runtime < 150)
            ng.UnlockMedal(68302);
        if (score >= 10000)
            ng.UnlockMedal(68303);
    }

    public void Restart()
    {
        StartCoroutine(LoadLevel(3));
    }

    public void LoadTutorial()
    {
        PlayerPrefs.SetInt("SLAMMA_HAS_PLAYED_TUTORIAL", 0);
        StartCoroutine(LoadLevel(2));
    }

    public void BakeNavigation()
    {
        navBaker.RebuildNavMesh();
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        if (loadingLevel)
            yield break;

        loadingLevel = true;
        ply.states.canMove = false;
        fadeAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        fadeAnimator.Play("BlackFadeOut");
        yield return new WaitForSecondsRealtime(0.8f);
        if (musicSource != null && !musicSource.isPlaying)
            musicSource.Play();

        Time.timeScale = 1;
        Application.LoadLevel(levelIndex);
    }

    public void SetPausedState(bool paused)
    {
        if (loadingLevel || showingResults)
            return;

        gamePaused = paused;
        ply.states.canMove = !paused;

        if (paused)
        {
            Cursor.lockState = CursorLockMode.None;
            pauseScreen.anchoredPosition = Vector2.zero;
            hudScreen.anchoredPosition = Vector2.up * 2000;
            Time.timeScale = 0;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            pauseScreen.anchoredPosition = Vector2.up * 1000;
            hudScreen.anchoredPosition = Vector2.zero;
            Time.timeScale = 1;
        }
    }

    public void EnterNewRoom(Transform openedDoorTransform)
    {
        int anchor = currentRoom.GetAnchorIndexFromTransform(openedDoorTransform);

        if (anchor == 0 && currentRoom == exitRoom)
        {
            EnterFinalRoom();
            return;
        }

        RoomManager newRoom;
        if (currentRoom.hardToReachDoors[anchor] || (ply.holdingDoor && Random.Range(0, 1f) > 0.8f))
            newRoom = GetValidRoom(GetOppositeAnchor(anchor), hardToReachRoomPrefabPool);
        else
            newRoom = GetValidRoom(GetOppositeAnchor(anchor), roomPrefabPool);

        RoomManager r = Instantiate(newRoom, Vector3.down * 25, Quaternion.identity).GetComponent<RoomManager>();
        print(anchor);
        StartCoroutine(r.SpawnAtLocation(GetOppositeAnchor(anchor), currentRoom));

        roomsCleared++;

        // Check if we should have the exit sign appear
        if (score >= scoreThreshold && r.anchors[0] != null && anchor != 1 && Random.Range(0, 1f) >= 0.5f)
        {
            exitRoom = r;
            r.SpawnExitSign();
        }

        //Transform link = Instantiate(navLinkPrefab, r.anchors[GetOppositeAnchor(anchor)]).transform;
        //link.localPosition = new Vector3(0, -1.75f, 0);
        //link.localEulerAngles = new Vector3(0, -180, 0);

        currentRoom.connections[anchor] = r;
        currentRoom.ResetAllOtherDoors(anchor);
        currentRoom = r;
    }

    public void EnterFinalRoom()
    {
        DoorScript[] doors = FindObjectsOfType<DoorScript>();
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        if (musicSource != null)
            musicSource.Stop();

        RoomManager r = Instantiate(endRoom, Vector3.down * 25, Quaternion.identity).GetComponent<RoomManager>();
        StartCoroutine(r.SpawnAtLocation(1, currentRoom));
        currentRoom.connections[0] = r;
        currentRoom.ResetAllOtherDoors(0);
        currentRoom = r;

        for (int i = 0; i < doors.Length; i++)
            doors[i].locked = true;
        for (int i = 0; i < enemies.Length; i++)
            Destroy(enemies[i].gameObject);
    }

    RoomManager GetValidRoom(int requiredIndex, List<RoomManager> roomPool)
    {
        List<RoomManager> rooms = new List<RoomManager>(roomPool);
        RoomManager r = null;

        // precondition: there's AT LEAST ONE valid room in the pool
        while (r == null && rooms.Count > 0)
        {
            RoomManager s = rooms[Random.Range(0, rooms.Count)];
            if (s.anchors[requiredIndex] == null || s.id == currentRoom.id || visitedRooms.Contains(s))
            {
                rooms.Remove(s);

                if (rooms.Count == 0)
                {
                    print("Reset");
                    break;
                }
            }
            else
            {
                r = s;
                visitedRooms.Add(s);
            }
        }

        if (rooms.Count == 0)
        {
            rooms = new List<RoomManager>(roomPool);
            visitedRooms = new List<RoomManager>();
            r = roomPrefabPool[0]; // load square room on default
        }

        return r;
    }

    public void UnlockMedal(int id)
    {
        ng.UnlockMedal(id);
    }

    public void UpdateCurrentRoom(RoomManager r)
    {
        currentRoom = r;
    }

    int GetOppositeAnchor(int anchor)
    {
        switch (anchor)
        {
            case 0:
                return 1;
            case 1:
                return 0;
            case 2:
                return 3;
            case 3:
                return 2;
        }
        return -1;
    }

    public void IncreaseScore(int amount, string description)
    {
        currentComboPoints += amount;
        attackDescriptionText.text = description;
        attackDescriptionText.fontSize = 55;
        if (currentComboPoints > 200 * Mathf.Pow(scoreMultiplier, 1.1f))
        {
            scoreMultiplierText.fontSize = 90;
            currentComboPoints = 0;
            scoreMultiplier = Mathf.Clamp(scoreMultiplier + 1, 1, 4);
            scoreMultiplierText.text = scoreMultiplier + "x";
        }

        if (currentMultiplierCountdown != null)
            StopCoroutine(currentMultiplierCountdown);

        currentMultiplierCountdown = ShowAttackDescription();
        StartCoroutine(currentMultiplierCountdown);

        IncreaseScore(amount * scoreMultiplier);
    }

    IEnumerator ShowAttackDescription()
    {
        yield return new WaitForSeconds(7f - (scoreMultiplier * 0.5f));
        attackDescriptionText.text = "";
        currentComboPoints = 0;
        scoreMultiplier = 1;
        scoreMultiplierText.text = "1x";
    }

    public void IncreaseScore(int amount)
    {
        score += amount;
    }

    public void UpdateMusicVolume()
    {
        if (musicSlider == null)
            return;

        int volume = (int)musicSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_MUS_VOLUME", volume);
        mixer.SetFloat("MusicVolume", volume);
    }
    public void UpdateSFXVolume()
    {
        if (sfxSlider == null)
            return;

        int volume = (int)sfxSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_SFX_VOLUME", volume);
        mixer.SetFloat("SFXVolume", volume);
    }

    public void UpdateAmbienceVolume()
    {
        if (ambSlider == null)
            return;

        int volume = (int)ambSlider.value * 4;

        if (volume == -40)
            volume = -80;

        PlayerPrefs.SetInt("SLAMMA_AMB_VOLUME", volume);
        mixer.SetFloat("AmbienceVolume", volume);
    }

    public void LoadVolumeFromPlayerPrefs()
    {
        if (PlayerPrefs.HasKey("SLAMMA_AMB_VOLUME"))
        {
            musicSlider.value = PlayerPrefs.GetInt("SLAMMA_MUS_VOLUME") / 4;
            sfxSlider.value = PlayerPrefs.GetInt("SLAMMA_SFX_VOLUME") / 4;
            ambSlider.value = PlayerPrefs.GetInt("SLAMMA_AMB_VOLUME") / 4;
        }

        UpdateMusicVolume();
        UpdateSFXVolume();
        UpdateAmbienceVolume();
    }


    // Update is called once per frame
    void Update()
    {
        if (!initialized)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            SetPausedState(!gamePaused);

        if (!shownThreshold && score >= scoreThreshold)
        {
            shownThreshold = true;
            StartCoroutine(ShowThresholdText());
        }

        // Update score text
        displayedScore = Mathf.Lerp(displayedScore, score, 0.05f);
        scoreText.text = Mathf.RoundToInt(displayedScore).ToString();
        scoreSlider.value = Mathf.Lerp(scoreSlider.value, Mathf.Clamp01((float)score / scoreThreshold), 0.05f);

        scoreMultiplierText.fontSize = Mathf.Lerp(scoreMultiplierText.fontSize, 44.5f, 0.1f);
        attackDescriptionText.fontSize = Mathf.Lerp(attackDescriptionText.fontSize, 44.5f, 0.1f);


        int targetFontSize = 90;
        if (Mathf.RoundToInt(displayedScore) != score)
            scoreText.fontSize = 110;

        scoreText.fontSize = Mathf.Lerp(scoreText.fontSize, targetFontSize, 0.05f);

        if (timerRunning)
        {
            runtime += Time.deltaTime;
            float timer = runtime;
            int minutes = Mathf.FloorToInt(timer / 60F);
            int seconds = Mathf.FloorToInt(timer - minutes * 60);
            int milliseconds = Mathf.FloorToInt(((timer - (minutes * 60) - seconds)) * 100);
            string niceTime = string.Format("{0:00}:{1:00}:{2:00}", minutes, seconds, milliseconds);
            timerText.text = niceTime;
        }

        // Update health
        for (int i = 0; i < ply.health; i++)
            hearts[i].color = Color.white;
        for (int i = ply.health; i < 3; i++)
            hearts[i].color = Color.black;

        if (ply.doorCharges == 0)
        {
            for (int i = 0; i < 3; i++)
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

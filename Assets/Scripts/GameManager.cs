using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AI;
public class GameManager : MonoBehaviour
{
    Camera camObject;
    Transform cam;
    Image[] hearts;
    Image[] doors;
    Slider scoreSlider;

    TextMeshProUGUI roomNumberText;
    TextMeshProUGUI scoreText;

    RectTransform tipBubble;
    Transform tipBubblePointer;

    PlayerController ply;

    public int score;
    public int scoreThreshold = 2500;
    public float displayedScore = 0;
    public int roomsCleared;

    public Transform globalCameraReference;
    public GameObject doorPrefab;
    public GameObject cellDoorPrefab;
    public GameObject prisonerPrefab;
    public GameObject copPrefab;
    public GameObject exitSignPrefab;
    public GameObject navLinkPrefab;

    public List<RoomManager> roomPrefabPool;
    public List<RoomManager> hardToReachRoomPrefabPool; // Accessed from doors you need to slam your way into, also appear at lower rates if you're just holding a door
    public RoomManager endRoom;
    public RoomManager currentRoom;

    List<RoomManager> spawnedRooms;
    List<RoomManager> visitedRooms;

    AudioSource musicSource;

    RoomManager exitRoom;

    // Start is called before the first frame update
    void Start()
    {
        roomNumberText = GameObject.Find("RoomNumberText").GetComponent<TextMeshProUGUI>();
        scoreText = GameObject.Find("PointsText").GetComponent<TextMeshProUGUI>();

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
        tipBubble = GameObject.Find("TipBubble").GetComponent<RectTransform>();
        tipBubblePointer = tipBubble.GetChild(0);
        Cursor.lockState = CursorLockMode.Locked;
        cam = GameObject.Find("CameraAnimations").transform;
        camObject = GameObject.Find("Camera").GetComponent<Camera>();

        //Time.timeScale = 0;
        StartCoroutine(HideTipText()); // Temp
    }

    IEnumerator HideTipText()
    {
        yield return new WaitForSeconds(5);
        Destroy(GameObject.Find("TipText"));
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
        if(currentRoom.hardToReachDoors[anchor] || (ply.holdingDoor && Random.Range(0, 1f) > 0.8f))
            newRoom = GetValidRoom(GetOppositeAnchor(anchor), hardToReachRoomPrefabPool);
        else
            newRoom = GetValidRoom(GetOppositeAnchor(anchor), roomPrefabPool);

        RoomManager r = Instantiate(newRoom, Vector3.down * 25, Quaternion.identity).GetComponent<RoomManager>();
        print(anchor);
        StartCoroutine(r.SpawnAtLocation(GetOppositeAnchor(anchor), currentRoom));

        roomsCleared++;

        // Check if we should have the exit sign appear
        if (score >= 2500 && r.anchors[0] != null && anchor != 1 && Random.Range(0, 1f) >= 0.5f)
        {
            exitRoom = r;
            r.SpawnExitSign();
        }

        Transform link = Instantiate(navLinkPrefab, r.anchors[GetOppositeAnchor(anchor)]).transform;
        link.localPosition = new Vector3(0, -1.75f, 0);
        link.localEulerAngles = new Vector3(0, -180, 0);

        currentRoom.connections[anchor] = r;
        currentRoom.ResetAllOtherDoors(anchor);
        currentRoom = r;

        roomNumberText.text = "Room " + roomsCleared;
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
                    rooms = new List<RoomManager>(roomPool);
                    visitedRooms = new List<RoomManager>();
                }
            }
            else
            {
                r = s;
                visitedRooms.Add(s);
            }
        }

        return r;
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

    public void IncreaseScore(int amount)
    {
        score += amount;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();

        // Update score text
        displayedScore = Mathf.Lerp(displayedScore, score, 0.05f);
        scoreText.text = Mathf.RoundToInt(displayedScore).ToString();
        scoreSlider.value = Mathf.Lerp(scoreSlider.value, Mathf.Clamp01((float)score / scoreThreshold), 0.05f);

        int targetFontSize = 90;
        if (Mathf.RoundToInt(displayedScore) != score)
            scoreText.fontSize = 110;

        scoreText.fontSize = Mathf.Lerp(scoreText.fontSize, targetFontSize, 0.05f);

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
    // Use this for initialization
    public void BakeNavMesh()
    {
        NavMeshSurface[] surfaces = FindObjectsOfType<NavMeshSurface>();
        

        /*for (int j = 0; j < objectsToRotate.Length; j++)
        {
            objectsToRotate[j].localRotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
        }*/

        for (int i = 0; i < surfaces.Length; i++)
        {
            surfaces[i].BuildNavMesh();
        }
    }

    public void DisplayTipBubble(Vector3 castPosition, Transform hitObject)
    {
        float screenScale = Screen.height / 768f;
        tipBubble.anchoredPosition = camObject.WorldToScreenPoint(castPosition);
        Quaternion lookAt = Quaternion.LookRotation(hitObject.transform.position - castPosition);
        lookAt.x = 0;
        lookAt.y = 0;
        tipBubblePointer.rotation = lookAt;
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

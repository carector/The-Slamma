using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    Camera camObject;
    Transform cam;
    Image[] hearts;
    Image[] doors;

    RectTransform tipBubble;
    Transform tipBubblePointer;

    PlayerController ply;

    public int score;
    public Transform globalCameraReference;
    public GameObject doorPrefab;
    public GameObject cellDoorPrefab;
    public GameObject prisonerPrefab;
    public GameObject copPrefab;
    public List<RoomManager> roomPrefabPool;
    public RoomManager currentRoom;

    List<RoomManager> spawnedRooms;
    List<RoomManager> visitedRooms;

    // Start is called before the first frame update
    void Start()
    {
        visitedRooms = new List<RoomManager>();
        spawnedRooms = new List<RoomManager>();
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
    }

    public void EnterNewRoom(Transform openedDoorTransform)
    {
        int anchor = currentRoom.GetAnchorIndexFromTransform(openedDoorTransform);
        RoomManager newRoom = GetValidRoom(GetOppositeAnchor(anchor));
        RoomManager r = Instantiate(newRoom, Vector3.down * 25, Quaternion.identity).GetComponent<RoomManager>();
        print(anchor);
        r.SpawnAtLocation(GetOppositeAnchor(anchor), currentRoom);
        currentRoom.connections[anchor] = r;
        currentRoom.ResetAllOtherDoors(anchor);
        currentRoom = r;
    }

    RoomManager GetValidRoom(int requiredIndex)
    {
        List<RoomManager> rooms = new List<RoomManager>(roomPrefabPool);
        RoomManager r = null;

        while (r == null && rooms.Count > 0)
        {
            RoomManager s = rooms[Random.Range(0, rooms.Count)];
            if (s.anchors[requiredIndex] == null || s.id == currentRoom.id || visitedRooms.Contains(s))
            {
                rooms.Remove(s);

                if (rooms.Count == 0)
                {
                    print("Reset");
                    rooms = new List<RoomManager>(roomPrefabPool);
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

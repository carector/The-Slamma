using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public Transform[] anchors = new Transform[4];
    public RoomManager[] connections = new RoomManager[4]; // N S E W
    public DoorScript[] doors = new DoorScript[4];
    public Transform[] cellDoorSpawnPoints;
    public Transform[] prisonerSpawnPoints;
    public Transform[] copSpawnPoints;
    public float enemySpawnChance = 0.5f;
    public float copSpawnChance = 0.25f;
    public string id;



    public List<Transform> spawnedEnemies;

    public Transform[] spawnedCellDoors;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (gm != null)
            return;

        spawnedEnemies = new List<Transform>();
        if(spawnedCellDoors.Length == 0)
            spawnedCellDoors = new Transform[cellDoorSpawnPoints.Length];

        gm = FindObjectOfType<GameManager>();
    }

    public int GetAnchorIndexFromTransform(Transform t) // Can be door transform or anchor transform
    {
        for (int i = 0; i < 4; i++)
            if ((anchors[i] != null && t == anchors[i]) || (doors[i] != null && t == doors[i].transform))
                return i;
        return -1;
    }

    public void ResetAllOtherDoors(int index)
    {
        for (int i = 0; i < 4; i++)
        {
            if (doors[i] != null && i != index)
            {
                doors[i].ResetDoor();
                if (connections[i] != null)
                {
                    connections[i].DeleteRoom(GetOppositeAnchor(i));
                    connections[i] = null;
                }
            }
        }
    }

    public void SpawnExitSign()
    {
        if (anchors[0] == null)
            return;
        Transform sign = Instantiate(gm.exitSignPrefab, anchors[0]).transform;
        sign.localPosition = new Vector3(-1f, 2.15f, -0.5f);
    }

    public IEnumerator SpawnAtLocation(int compassDir, RoomManager connection)
    {
        Initialize();

        connections = new RoomManager[4];
        doors = new DoorScript[4];

        // Set position based on door
        Transform offset = anchors[compassDir];
        connections[compassDir] = connection;
        Vector3 offsetPos = transform.position - offset.position;// + Vector3.down*2.35f;
        transform.position = connection.anchors[GetOppositeAnchor(compassDir)].position + offsetPos;

        yield return new WaitForEndOfFrame();

        // Check for existing doors, then spawn new ones for this room
        doors[compassDir] = connection.doors[GetOppositeAnchor(compassDir)];
        for (int i = 0; i < 4; i++)
            if (i != compassDir && anchors[i] != null)
                doors[i] = Instantiate(gm.doorPrefab, anchors[i].position, anchors[i].rotation).GetComponentInChildren<DoorScript>();

        // Also spawn cell doors
        for (int i = 0; i < cellDoorSpawnPoints.Length; i++)
            spawnedCellDoors[i] = Instantiate(gm.cellDoorPrefab, cellDoorSpawnPoints[i].position, cellDoorSpawnPoints[i].rotation).transform;

        // Spawn prisoners
        for (int i = 0; i < prisonerSpawnPoints.Length; i++)
            if (Random.Range(0, 1f) <= enemySpawnChance)
                spawnedEnemies.Add(Instantiate(gm.prisonerPrefab, prisonerSpawnPoints[i].position, Quaternion.identity).transform);

        // Spawn cops
        for (int i = 0; i < copSpawnPoints.Length; i++)
            if (Random.Range(0, 1f) <= copSpawnChance)
                spawnedEnemies.Add(Instantiate(gm.copPrefab, copSpawnPoints[i].position, Quaternion.identity).transform);
    }

    public void DeleteRoom(int doorToKeep)
    {
        for (int i = 0; i < 4; i++)
            if (doors[i] != null && i != doorToKeep)
                doors[i].DeleteDoor();
        for (int i = 0; i < spawnedCellDoors.Length; i++)
            if(spawnedCellDoors[i] != null)
                Destroy(spawnedCellDoors[i].gameObject);

        foreach (Transform t in spawnedEnemies)
            if (t != null)
                Destroy(t.gameObject);

        Destroy(this.gameObject);
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

    private void OnTriggerStay(Collider other)
    {
        if (gm.currentRoom != this && other.tag == "Player")
            gm.UpdateCurrentRoom(this);
    }
}

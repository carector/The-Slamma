using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorRespawner : MonoBehaviour
{
    public Transform door;
    PlayerController ply;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(door == null && !ply.holdingDoor)
        {
            door = Instantiate(gm.cellDoorPrefab, transform.position, transform.rotation).transform;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEndTrigger : MonoBehaviour
{
    PlayerController ply;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
        gm = FindObjectOfType<GameManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ply.states.canMove = false;
            gm.ShowResultsScreen();
        }
    }
}

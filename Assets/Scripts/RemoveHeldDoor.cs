using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveHeldDoor : MonoBehaviour
{
    PlayerController ply;
    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            ply.doorCharges = 0;
            Destroy(this.gameObject);
        }
    }
}

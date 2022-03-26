using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestructAfterTime : MonoBehaviour
{
    public float timeBeforeDeath = 5;

    // Start is called before the first frame update
    void Start()
    {
        transform.parent = FindObjectOfType<GameManager>().currentRoom.transform;
        StartCoroutine(DeathCountdown());
    }

    IEnumerator DeathCountdown()
    {
        yield return new WaitForSeconds(timeBeforeDeath);
        Destroy(this.gameObject);
    }
}

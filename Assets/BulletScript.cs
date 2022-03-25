using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    PlayerController ply;
    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.forward * 0.075f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == (1 << 11))
            Destroy(this.gameObject);
        if (other.gameObject.tag == "Player")
        {
            ply.TakeDamage();
            Destroy(this.gameObject);
        }
    }
}

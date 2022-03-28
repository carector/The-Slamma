using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    public float speed = 0.1f;
    PlayerController ply;
    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position += transform.forward * speed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (other.GetComponent<CopEnemy>() == null)
            {
                other.GetComponent<Enemy>().GetHitByAttack((other.transform.position - transform.position).normalized * 50);
                Destroy(this.gameObject);
            }
        }
        else
        {
            if (other.gameObject.tag == "Player")
            {
                ply.TakeDamage();
                Destroy(this.gameObject);
            }

            if (other.gameObject.tag != "Room")
                Destroy(this.gameObject);
        }
    }
}

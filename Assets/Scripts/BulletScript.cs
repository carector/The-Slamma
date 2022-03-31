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

    private void Update()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, 0.35f);
        for (int i = 0; i < cols.Length; i++)
            if (cols[i].tag != "Room" && cols[i].tag != "Player" && cols[i].tag != "Enemy" && cols[i].tag != "Door" && cols[i].transform != transform)
                Destroy(this.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            if (other.GetComponent<CopEnemy>() == null)
            {
                other.GetComponent<Enemy>().GetHitByAttack((other.transform.position - transform.position).normalized * 50, false, true);
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

            if(other.gameObject.tag == "Door")
            {
                DoorScript d = other.GetComponent<DoorScript>();
                if (!d.lethal)
                    d.AddDoorForce(transform, false);
                Destroy(this.gameObject);
            }

            if (other.gameObject.tag != "Room")
                Destroy(this.gameObject);
        }
    }
}

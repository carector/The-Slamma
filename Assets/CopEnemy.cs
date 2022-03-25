using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopEnemy : Enemy
{
    public bool shooting;
    public GameObject bullet;
    public AudioClip shootSound;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
    }

    void Update()
    {
        if (!states.dying && LineOfSightOnPlayer() && !shooting)
        {
            NavmeshMoveTowards(ply.transform);

            if (Vector3.Distance(transform.position, ply.transform.position) < 7)
            {
                anim.Play("Cop_Ready");
                shooting = true;
            }
        }
        else
            nav.isStopped = true;

        if (Vector3.Distance(transform.position, ply.transform.position) < 1.5f && !ply.states.takingDamage)
            ply.TakeDamage();
    }

    public void EndShootState()
    {
        shooting = false;
    }
    public void SpawnBullet()
    {
        audio.PlayOneShot(shootSound);
        Transform t = Instantiate(bullet, transform.position + transform.forward * 0.25f + transform.up, Quaternion.identity).transform;
        t.forward = transform.forward;
    }


}

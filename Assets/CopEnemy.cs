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
            nav.isStopped = false;
            anim.SetFloat("WalkSpeed", nav.velocity.magnitude / nav.speed);
            NavmeshMoveTowards(ply.transform);

            if (Vector3.Distance(transform.position, ply.transform.position) < 15)
            {
                anim.Play("Cop_Ready");
                shooting = true;
            }
        }
        else
            nav.isStopped = true;

        if (states.dying)
        {
            nav.velocity = Vector3.Lerp(nav.velocity, Vector3.zero, 0.025f);
            nav.isStopped = false;
        }
    }

    public void EndShootState()
    {
        print("Stopped shooting");
        shooting = false;
    }
    public void SpawnBullet()
    {
        audio.PlayOneShot(shootSound);
        Transform t = Instantiate(bullet, transform.position + transform.forward * 0.25f + transform.up*1.4f + transform.right*0.25f, Quaternion.identity).transform;
        Vector3 bulletDir = ply.transform.position - transform.position;
        bulletDir.y = 0;
        t.forward = (bulletDir);
    }


}

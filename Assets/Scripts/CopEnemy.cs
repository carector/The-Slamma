using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopEnemy : Enemy
{
    public bool shooting;
    public GameObject bullet;
    public AudioClip shootSound;

    Transform spriteTransform;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        spriteTransform = transform.GetChild(0);

        if (gm.score >= gm.scoreThreshold)
        {
            nav.speed *= 1.5f;
            anim.SetFloat("ShootSpeed", 0.85f);
        }
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
            nav.velocity = Vector3.Lerp(nav.velocity, Vector3.zero, 0.05f);
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
        Transform t = Instantiate(bullet, spriteTransform.position + spriteTransform.forward * 0.25f + spriteTransform.up * 0.25f - spriteTransform.right * 0.25f, Quaternion.identity).transform;
        Vector3 bulletDir = (ply.transform.position - Vector3.up) - transform.position;
        //bulletDir.y = 0;
        t.forward = (bulletDir);
    }


}

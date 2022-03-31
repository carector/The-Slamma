using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerEnemy : Enemy
{
    public CopEnemy nearestCop;

    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
        StartCoroutine(ScanForCopCycle());

        if (gm.score >= gm.scoreThreshold)
        {
            nav.speed *= 1.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // auto-slow from big knockbacks, movement is done by navmesh
        if (states.dying)
            nav.velocity = Vector3.Lerp(nav.velocity, Vector3.zero, 0.05f);

        if (!states.dying)
        {
            anim.SetFloat("WalkSpeed", nav.velocity.magnitude / 5);
            if (nearestCop != null && LineOfSightOnTransform(nearestCop.transform) && Mathf.Abs(transform.position.y - nearestCop.transform.position.y) < 4)
                NavmeshMoveTowards(nearestCop.transform, 8);
            else if (LineOfSightOnPlayer())
                NavmeshMoveTowards(ply.transform, 5);
        }

        if (nearestCop != null && Vector3.Distance(transform.position, nearestCop.transform.position) < 2f && !nearestCop.states.dying)
        {
            ply.PlaySFX(4);
            nearestCop.GetHitByAttack((nearestCop.transform.position - transform.position+transform.up).normalized * 50, false, true);
        }

        if (Vector3.Distance(transform.position, ply.transform.position) < 1.33f && !ply.states.takingDamage && !states.dying)
            ply.TakeDamage();
    }

    IEnumerator ScanForCopCycle()
    {
        float shortestDistance = 100;

        Collider[] cols = Physics.OverlapSphere(transform.position, 30, 1 << 9);
        for (int i = 0; i < cols.Length; i++)
            if (cols[i].GetComponent<CopEnemy>() != null && Vector3.Distance(cols[i].transform.position, transform.position) < shortestDistance)
            {
                nearestCop = cols[i].transform.GetComponent<CopEnemy>();
                shortestDistance = Vector3.Distance(cols[i].transform.position, transform.position);
            }

        yield return new WaitForSeconds(3);
        StartCoroutine(ScanForCopCycle());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Door")
        {
            DoorScript d = other.GetComponent<DoorScript>();
            if(!d.lethal)
                d.AddDoorForce(transform, false);
        }
    }
}

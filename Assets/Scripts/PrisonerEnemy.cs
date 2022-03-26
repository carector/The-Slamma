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
    }

    // Update is called once per frame
    void Update()
    {
        // auto-slow from big knockbacks, movement is done by navmesh
        if (states.dying)
            nav.velocity = Vector3.Lerp(nav.velocity, Vector3.zero, 0.025f);

        if (!states.dying)
        {
            anim.SetFloat("WalkSpeed", nav.velocity.magnitude / 5);
            if (nearestCop != null && LineOfSightOnTransform(nearestCop.transform))
                NavmeshMoveTowards(nearestCop.transform, 8);
            else if (LineOfSightOnPlayer())
                NavmeshMoveTowards(ply.transform, 5);
        }

        if (nearestCop != null && Vector3.Distance(transform.position, nearestCop.transform.position) < 2f && !nearestCop.states.dying)
        {
            ply.PlaySFX(4);
            nearestCop.GetHitByAttack((nearestCop.transform.position - transform.position+transform.up).normalized * 50);
        }

        if (Vector3.Distance(transform.position, ply.transform.position) < 2f && !ply.states.takingDamage)
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
}

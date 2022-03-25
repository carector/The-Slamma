using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerEnemy : Enemy
{
    // Start is called before the first frame update
    void Start()
    {
        GetReferences();
    }

    // Update is called once per frame
    void Update()
    {
        if (!states.dying && LineOfSightOnPlayer())
            NavmeshMoveTowards(ply.transform);

        if (Vector3.Distance(transform.position, ply.transform.position) < 1.5f && !ply.states.takingDamage)
            ply.TakeDamage();
    }
}

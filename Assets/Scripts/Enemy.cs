using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    /* === Universal enemy requirements ===
     * 
     * Can be damaged in their own unique ways (TakeDamagePrereqs method is abstract, TakeDamage method is not)
     * Has a sprite child with SpriteDirectionalManager component
     * 
     * 
     * 
     */

    [System.Serializable]
    public class EnemyStates
    {
        public bool walking;
        public bool jumping;
        public bool takingDamage;
        public bool recoveringAttack;
        public bool dying;
    }


    public EnemyStates states;
    public int currentHealth;
    public GameObject grave;

    protected Animator anim;
    protected NavMeshAgent nav;
    protected SpriteRenderer spr;
    protected PlayerController ply;
    protected SpriteDirectionalManager sprDir;
    protected Rigidbody rb;

    private void Start()
    {
        GetReferences();

    }
    protected void GetReferences()
    {
        anim = GetComponentInChildren<Animator>();
        nav = GetComponent<NavMeshAgent>();
        spr = GetComponentInChildren<SpriteRenderer>();
        ply = FindObjectOfType<PlayerController>();
        sprDir = GetComponentInChildren<SpriteDirectionalManager>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!states.dying && LineOfSightOnPlayer())
            NavmeshMoveTowards(ply.transform);

        if (Vector3.Distance(transform.position, ply.transform.position) < 1.5f && !ply.states.takingDamage)
            ply.TakeDamage();
    }

    public bool TakeDamagePrereqs()
    {
        return true;
    }

    // Take damage sequence:
    // 1. Player calls GetHitByAttack
    // 2. TakeDamagePrereqs varies per enemy
    // 3. Additional method called when enemy gets hit? (TakeDamagePrereqs may be enough)
    public void GetHitByAttack(Vector3 velocity)
    {
        if (states.dying)
            return;
        if (TakeDamagePrereqs())
        {
            states.dying = true;
            StartCoroutine(Die(velocity));
        }
    }

    protected virtual bool LineOfSightOnPlayer()
    {

        bool visible = false;
        RaycastHit hit;
        if(Physics.Raycast(transform.position, (ply.transform.position - transform.position).normalized, out hit, 20))
            if (hit.transform.tag == "Player")
                visible = true;
        return visible;
    }

    public IEnumerator Die(Vector3 velocity)
    {
        GetComponentInChildren<Animator>().Play("EnemyDie");
        states.dying = true;
        nav.enabled = false;
        rb.isKinematic = false;
        rb.velocity = velocity;
        while(rb.velocity.magnitude > 1)
            yield return null;
        yield return new WaitForSeconds(0.5f);
        Instantiate(grave, transform.GetChild(0).position+Vector3.up*0.25f, Quaternion.identity);
        Destroy(this.gameObject);
    }

    // Navigation helper methods
    protected void NavmeshMoveTowards(Transform t)
    {
        if (nav == null)
        {
            if (GetComponent<NavMeshAgent>() == null)
                return;
            nav = GetComponent<NavMeshAgent>();
        }
        if (nav.isOnOffMeshLink && !states.jumping)
        {
            states.jumping = true;
            StartCoroutine(JumpMotionTest());
        }
        else
            nav.SetDestination(t.position);
        print("Moving");
    }

    IEnumerator JumpMotionTest()
    {
        if (spr == null)
        {
            if (GetComponentInChildren<SpriteRenderer>() == null)
                yield break;
            spr = GetComponentInChildren<SpriteRenderer>();
        }

        anim.Play("Test_Air");

        // Get start and end points of link
        Vector3 start = nav.currentOffMeshLinkData.startPos - (nav.currentOffMeshLinkData.startPos - transform.position);
        Vector3 end = nav.currentOffMeshLinkData.endPos + Vector3.up * nav.baseOffset;
        Vector3 vertex = (start + end) / 2;
        vertex.y = Mathf.Max(start.y, end.y) + 3;
        float initialDistance = Vector3.Distance(transform.position, end);
        float delta = 0;
        while (delta < 1)
        {
            transform.position = BezierPoint(start, end, vertex, delta);
            delta += Time.fixedDeltaTime * 1.5f;
            yield return new WaitForFixedUpdate();
        }

        nav.Warp(end);
        transform.rotation = Quaternion.Euler(0, GetAngleToPlayer(), 0);
        states.jumping = false;
    }

    // Used for jump motion between nav links
    public Vector3 BezierPoint(Vector3 start, Vector3 end, Vector3 vertex, float delta) // delta being a number between 0 and 1
    {
        // what?
        return Mathf.Pow((1 - delta), 2) * start + 2 * (1 - delta) * delta * vertex + Mathf.Pow(delta, 2) * end;
    }

    public float GetAngleToPlayer()
    {
        Vector3 lookAt = ply.transform.position - transform.position;
        return Quaternion.LookRotation(lookAt).eulerAngles.y;
    }

    bool WithinRange(float value, float low, float high)
    {
        return value >= low && value <= high;
    }

    // Used to determine if camera is to the left or right of the player
    float AngleDir(Vector3 fwd, Vector3 targetDir, Vector3 up)
    {
        Vector3 perp = Vector3.Cross(fwd, targetDir);
        float dir = Vector3.Dot(perp, up);

        if (dir < 0f)
            return -1f;
        else
            return 1f;
    }
}

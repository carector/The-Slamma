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

    public string animationPrefix;
    public EnemyStates states;
    public int currentHealth;
    public GameObject grave;

    protected AudioSource audio;
    protected Animator anim;
    protected NavMeshAgent nav;
    protected SpriteRenderer spr;
    protected PlayerController ply;
    protected SpriteDirectionalManager sprDir;
    protected Rigidbody rb;
    protected GameManager gm;
    protected Collider col;

    bool hasNoticedTarget;
    float origSpeed;
    bool linking;
    private void Start()
    {
        GetReferences();
    }

    IEnumerator WaitForNavMeshCreation()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        nav.enabled = true;
    }

    protected void GetReferences()
    {
        gm = FindObjectOfType<GameManager>();
        audio = GetComponent<AudioSource>();
        anim = GetComponentInChildren<Animator>();
        nav = GetComponent<NavMeshAgent>();
        if (nav != null)
            origSpeed = nav.speed;
        spr = GetComponentInChildren<SpriteRenderer>();
        ply = FindObjectOfType<PlayerController>();
        sprDir = GetComponentInChildren<SpriteDirectionalManager>();
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        StartCoroutine(WaitForNavMeshCreation());
    }

    public bool TakeDamagePrereqs()
    {
        return true;
    }

    // Take damage sequence:
    // 1. Player calls GetHitByAttack
    // 2. TakeDamagePrereqs varies per enemy
    // 3. Additional method called when enemy gets hit? (TakeDamagePrereqs may be enough)
    public void GetHitByAttack(Vector3 velocity, bool killedByHeldDoor, bool killedByAnotherEnemy)
    {
        if (states.dying)
            return;

        if (TakeDamagePrereqs())
        {
            states.dying = true;
            if(killedByHeldDoor)
                gm.IncreaseScore(100, "Slam");
            else if(killedByAnotherEnemy)
                gm.IncreaseScore(200, "Helping hand");
            else
                gm.IncreaseScore(150, "Stationary slam");

            StartCoroutine(Die(velocity));
        }
    }

    protected virtual bool LineOfSightOnPlayer()
    {
        if (states.dying)
            return false;

        if (hasNoticedTarget)
            return true;

        RaycastHit hit;
        //Debug.DrawRay(transform.position+transform.up, (ply.transform.position - transform.position).normalized * 20, Color.red, Time.deltaTime);
        if (Physics.Raycast(transform.position + transform.up, (ply.transform.position - (transform.position + transform.up)).normalized, out hit, 20, ~(1 << 9)))
        {
            if (hit.transform.tag == "Player")
            {
                NavMeshPath p = new NavMeshPath();
                if (nav == null || !nav.CalculatePath(ply.transform.position, p))
                    return false;

                hasNoticedTarget = true;
                StartCoroutine(LoSCooldown());
                return true;
            }
        }
        return false;
    }

    IEnumerator LoSCooldown()
    {
        yield return new WaitForSeconds(5);
        hasNoticedTarget = false;
    }

    protected virtual bool LineOfSightOnTransform(Transform t)
    {
        if (states.dying)
            return false;

        if (hasNoticedTarget)
            return true;

        RaycastHit hit;
        //Debug.DrawRay(transform.position+transform.up, (ply.transform.position - transform.position).normalized * 20, Color.red, Time.deltaTime);
        if (Physics.Raycast(transform.position + transform.up, (t.position - (transform.position + transform.up)).normalized, out hit, 20))
        {
            if (hit.transform == t)
            {
                NavMeshPath p = new NavMeshPath();
                if (nav == null || !nav.CalculatePath(t.position, p))
                    return false;

                hasNoticedTarget = true;
                StartCoroutine(LoSCooldown());
                return true;
            }
        }
        return false;
    }

    public IEnumerator Die(Vector3 velocity)
    {
        col.isTrigger = true;
        velocity.y = 0;
        GetComponentInChildren<Animator>().Play(animationPrefix + "Die");
        states.dying = true;
        nav.enabled = true;
        nav.velocity = velocity;
        while (nav.velocity.magnitude > 0.5f)
            yield return null;
        yield return new WaitForSeconds(0.35f);
        Instantiate(grave, transform.GetChild(0).position + Vector3.up * 0.25f, Quaternion.identity);
        Destroy(this.gameObject);
    }

    public void AddKnockback(Vector3 direction)
    {
        direction.y = 0;
        nav.velocity = direction * 10;
    }


    // Navigation helper methods
    protected void NavmeshMoveTowards(Transform t, float speed)
    {
        if (!linking)
            nav.speed = speed;

        NavmeshMoveTowards(t);
    }

    protected void NavmeshMoveTowards(Transform t)
    {
        if (!nav.enabled)
            return;

        if (nav == null)
        {
            if (GetComponent<NavMeshAgent>() == null)
                return;
            nav = GetComponent<NavMeshAgent>();
        }
        else
            nav.SetDestination(t.position);

        if (nav.isOnOffMeshLink)
        {
            Vector3 pos = nav.currentOffMeshLinkData.endPos;
            transform.position = Vector3.MoveTowards(transform.position, pos, 0.025f);
            if (transform.position == pos)
                nav.Warp(pos);
        }

        //print("Moving towards " + t.name);
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

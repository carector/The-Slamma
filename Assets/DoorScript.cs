using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    public bool opened;
    public bool canBeGrabbed;
    public bool lethal;
    public bool locked;
    public float bounciness = 0.35f;

    float lethalTimer = 0;
    public float sign = 1;
    PlayerController ply;
    Rigidbody rb;
    HingeJoint joint;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        joint = GetComponent<HingeJoint>();
        gm = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        ply = FindObjectOfType<PlayerController>();

        if (transform.parent.parent != null)
            transform.parent.parent = null;
    }

    // Update is called once per frame
    void Update()
    {
        lethal = lethalTimer > 0;
        if (lethalTimer > 0)
        {
            lethalTimer -= Time.deltaTime;
        }
    }

    public void RotateTowardsShutPosition(Vector3 point)
    {
        Vector3 dir = point - (transform.position + transform.right);
        rb.velocity = Vector3.Lerp(rb.velocity, dir * 7.5f, 1f);
    }

    public void ResetDoor() // Resets to pre-kicked-down state
    {
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        opened = false;
        transform.localPosition = new Vector3(-1, 0, 0);
        transform.localRotation = Quaternion.identity;

    }

    public void DeleteDoor()
    {
        Destroy(transform.parent.gameObject);
    }

    public void KickOpenDoor()
    {
        if (locked)
            return;

        if (!canBeGrabbed)
            gm.EnterNewRoom(transform);
        rb.isKinematic = false;
        gm.ScreenShake();
        AddDoorForce();
        opened = true;
    }

    void AddDoorForce()
    {
        bool neg = Vector3.Angle(ply.transform.forward, transform.parent.forward) >= 90;
        sign = 1;
        if (neg)
            sign = -1;
        //rb.angularVelocity = transform.up * 100 * -sign;
        rb.velocity = transform.forward * 25 * sign;
        lethalTimer = 0.75f;
    }

    public void SlamDoor()
    {
        JointLimits limits = joint.limits;
        limits.bounciness = bounciness;
        joint.limits = limits;
        AddDoorForce();
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Enemy" && lethal)
            other.GetComponent<Enemy>().GetHitByAttack((other.transform.position - transform.position).normalized * 50);
    }
}

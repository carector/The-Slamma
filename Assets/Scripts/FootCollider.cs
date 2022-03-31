﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootCollider : MonoBehaviour
{
    public PlayerController.GroundHitData hitData;
    public List<Collider> cols;
    public float maxSlope;
    public bool coyoteTimeActive;
    IEnumerator coyoteTimeCoroutine;

    GameManager gm;
    Rigidbody prb;
    PlayerController ply;

    // Start is called before the first frame update
    void Start()
    {
        gm = FindObjectOfType<GameManager>();
        ply = FindObjectOfType<PlayerController>();
        prb = ply.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        RaycastHit hit;
        Ray r = new Ray(transform.position, Vector3.down);

        // Check to make sure none of our colliders we've hit are too slope-y
        foreach (Collider c in cols)
        {
            if (c == null)
                continue;

            hitData.isGrounded = false;

            if (c.Raycast(r, out hit, 10000f))
            {
                hitData.isGrounded = true;
                return;
            }
        }

        if (cols.Count > 0)
        {
            hitData.isGrounded = true;
            coyoteTimeActive = false;
            if (coyoteTimeCoroutine != null)
                StopCoroutine(coyoteTimeCoroutine);
        }
        else
        {
            if (hitData.isGrounded)
            {
                coyoteTimeActive = true;
                coyoteTimeCoroutine = CoyoteTime();
                StartCoroutine(coyoteTimeCoroutine);
            }
            hitData.isGrounded = false;
        }
    }

    IEnumerator CoyoteTime()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        coyoteTimeActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ply.states.canMove)
            return;

        if (other.tag != "NoWalk" && other.tag != "Enemy" && other.tag != "Door" && other.tag != "Player" && other.tag != "Room")
            cols.Add(other);
    }
    private void OnTriggerExit(Collider other)
    {
        if (cols.Contains(other))
            cols.Remove(other);
    }
}

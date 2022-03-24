using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [System.Serializable]
    public class PlayerMovementSettings
    {
        public float walkSpeed = 10;
        public float runSpeed = 13;
        public float referenceGravity = 6;
        public float maxVelocityChange = 10;
        public float mouseSensitivity = 3;
    }

    [System.Serializable]
    public class GroundHitData
    {
        public bool isGrounded;
    }

    [System.Serializable]
    public class PlayerStates
    {
        public bool canMove = true;
        public bool isGrounded;
        public bool takingDamage;
    }
    [System.Serializable]
    public class PlayerInputValues
    {
        public Vector2 targetMovement;
        public Vector2 movement;
        public Vector2 look;
        public bool diving;
        public bool reeling;
    }

    public int health = 3;

    public int doorCharges;
    public bool holdingDoor;

    public PlayerMovementSettings movement;
    public PlayerStates states;

    public DoorScript currentDoor;
    public AudioClip[] sfx;
    AudioSource audio;

    PlayerInputValues inputs;
    Rigidbody rb;
    [HideInInspector]
    public Transform camHolder;
    FootCollider foot;
    GroundHitData hitData;
    GameManager gm;
    Animator camAnimations;
    Animator legAnimations;

    float xAxisClamp;
    bool swinging;


    // Start is called before the first frame update
    void Start()
    {
        audio = GetComponent<AudioSource>();
        legAnimations = GameObject.Find("HUD").GetComponent<Animator>();
        gm = FindObjectOfType<GameManager>();
        rb = GetComponent<Rigidbody>();
        foot = FindObjectOfType<FootCollider>();
        camHolder = transform.GetChild(0);
        camAnimations = camHolder.transform.GetChild(0).GetComponent<Animator>();
        gm.globalCameraReference = camHolder;
        inputs = new PlayerInputValues();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateButtonInputs();
        UpdateMouseLookAxes();
    }

    private void FixedUpdate()
    {
        // Get ground hit data from foot collider
        GetGroundHitData();
        UpdateInputAxes();
    }

    // Obtain input values
    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        inputs.movement = context.ReadValue<Vector2>();
    }
    public void OnLook(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        inputs.look = context.ReadValue<Vector2>();
    }

    void UpdateButtonInputs()
    {
        if (!states.canMove)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!holdingDoor)
            {
                DoorScript d = CheckForDoorRaycast();
                if (d != null)
                {
                    legAnimations.Play("Kick");
                    audio.PlayOneShot(sfx[UnityEngine.Random.Range(0, 3)]);
                    if (!d.opened)
                        d.KickOpenDoor();
                    else
                        d.SlamDoor();
                }
            }
            else if(!swinging)
            {
                swinging = true;
                camAnimations.Play("CameraDoorSwing", 0, 0);
            }

        }
        else if (Input.GetMouseButtonDown(1))
        {
            DoorScript d = CheckForDoorRaycast();
            if (d != null && !holdingDoor)
            {
                audio.PlayOneShot(sfx[UnityEngine.Random.Range(0, 3)]);
                camAnimations.Play("CameraDoorIdle", 0, 0);
                doorCharges = 3;
                Destroy(d.transform.parent.gameObject);
                holdingDoor = true;
            }
        }

        if(doorCharges == 0 && holdingDoor)
        {
            holdingDoor = false;
            camAnimations.Play("CameraIdle", 0, 0);
        }

    }

    DoorScript CheckForDoorRaycast()
    {
        int mask = ~(1 << 8 & 1 << 9);
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(camHolder.position, 0.5f, camHolder.forward, 4, mask);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.tag == "Door")
                return hit.transform.GetComponent<DoorScript>();
        }
        return null;
    }

    public void TakeDamage()
    {
        gm.ScreenShake();
        health--;
        if(health == 0)
        {
            StartCoroutine(DieCoroutine());
        }
        StartCoroutine(TakeDamageCoroutine());
    }

    IEnumerator DieCoroutine()
    {
        states.canMove = false;
        audio.PlayOneShot(sfx[5]);
        GameObject.Find("DeathScreen").GetComponent<Image>().color = new Color(1, 0, 0, 0.5f);
        yield return new WaitForSeconds(3.5f);
        Application.LoadLevel(Application.loadedLevel);
    }

    IEnumerator TakeDamageCoroutine()
    {
        audio.PlayOneShot(sfx[4]);
        states.takingDamage = true;
        yield return new WaitForSeconds(1.5f);
        states.takingDamage = false;
    }

    public void Lunge()
    {
        gm.ScreenShake();
        audio.PlayOneShot(sfx[3]);
        DoorScript d = CheckForDoorRaycast();
        if (d != null)
        { 
            if (!d.opened)
                d.KickOpenDoor();
            else
                d.SlamDoor();
        }
        rb.velocity = transform.forward * 100;
        RaycastHit[] hits;
        hits = Physics.SphereCastAll(camHolder.position, 0.5f, camHolder.forward, 6);
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.tag == "Enemy")
                hit.transform.GetComponent<Enemy>().GetHitByAttack((hit.transform.position - transform.position).normalized*50);
        }
    }

    public void FinishSwing()
    {
        doorCharges--;
        swinging = false;
    }

    void UpdateInputAxes()
    {
        // Move
        float moveHoriz = inputs.movement.x;
        float moveVert = inputs.movement.y;

        // Should ALWAYS be called, even if player is supposed to be frozen
        MovePlayer(moveHoriz, moveVert);
    }
    void UpdateMouseLookAxes()
    {
        // Look
        float lookHoriz = inputs.look.x;
        float lookVert = inputs.look.y;
        MouseLook(lookHoriz, lookVert);
    }

    void MovePlayer(float horiz, float vert)
    {
        if (!states.canMove)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, new Vector3(0, rb.velocity.y, 0), 0.25f);
            return;
        }

        Vector3 dir = transform.TransformDirection(new Vector3(horiz, 0, vert));


        if (states.isGrounded)
        {
            dir *= movement.walkSpeed;

            // Apply force that attempts to reach our target (input) velocity
            // Slow velocity down rather than snapping to max speed
            float maxSpeed = movement.maxVelocityChange;
            if (rb.velocity.magnitude > maxSpeed)
                maxSpeed = rb.velocity.magnitude;

            Vector3 velocityChange = Vector3.ClampMagnitude(Vector3.Lerp(Vector3.zero, (dir - rb.velocity), 0.25f), maxSpeed);
            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        // Apply additional gravity
        rb.AddForce(new Vector3(0, -movement.referenceGravity * rb.mass, 0));
    }

    int ClampInputToOne(float input)
    {
        if (input == 0)
            return 0;
        else if (input > 0)
            return 1;
        else
            return -1;
    }

    void MouseLook(float lookX, float lookY)
    {
        if (!states.canMove)
            return;

        // Multiply mouse positions by mouse sensitivity
        float rotAmountX = lookX * movement.mouseSensitivity;
        float rotAmountY = lookY * movement.mouseSensitivity;

        // Subtract our rotation amount in the y axis from our clamp value (more on this below)
        xAxisClamp -= rotAmountY;

        // Create directional values we can send to our camera so we can rotate it
        Vector3 targetRotCam = camHolder.rotation.eulerAngles;
        Vector3 targetRotBody = transform.rotation.eulerAngles;

        // Now subtract our mouse's change in position from our directional values
        targetRotCam.x -= rotAmountY;
        targetRotCam.z = 0;
        targetRotBody.y += rotAmountX;

        // Clamp the rotation in the X axis so our player can't accidentally look backwards and upside down
        if (xAxisClamp > 90f)
        {
            xAxisClamp = 90;
            targetRotCam.x = 90;
        }
        else if (xAxisClamp < -90)
        {
            xAxisClamp = -90;
            targetRotCam.x = 270;
        }

        // Finally, rotate our player
        camHolder.rotation = Quaternion.Euler(targetRotCam);
        transform.rotation = Quaternion.Euler(targetRotBody);
    }

    void GetGroundHitData()
    {
        hitData = foot.hitData;
        states.isGrounded = hitData.isGrounded;
    }
}
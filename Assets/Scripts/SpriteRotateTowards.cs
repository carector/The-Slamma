using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRotateTowards : MonoBehaviour
{
    public bool billboard;
    public Transform transformOverride;
    PlayerController ply;
    Transform cam;
    Transform focus;

    // Start is called before the first frame update
    void Start()
    {
        ply = FindObjectOfType<PlayerController>();
        focus = ply.transform;
        if (transformOverride != null)
            focus = transformOverride;
        cam = ply.camHolder;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        if (cam == null)
            cam = ply.camHolder;

        Vector3 rot;
        if (!billboard)
        {
            rot = Quaternion.LookRotation(transform.position - focus.position).eulerAngles;
            rot.x = 0; rot.z = 0;
        }
        else
            rot = cam.eulerAngles;

        transform.rotation = Quaternion.Euler(rot);
    }
}

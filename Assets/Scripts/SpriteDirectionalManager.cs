using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteDirectionalManager : MonoBehaviour
{
    public bool animate;
    public bool useDirectionalAnimations = true;
    public string animationPrefix;

    Transform body;
    Animator anim;
    SpriteRenderer spr;
    GameManager gm;
    PlayerController ply;

    // Start is called before the first frame update
    void Start()
    {
        body = transform.parent;
        ply = FindObjectOfType<PlayerController>();
        spr = GetComponent<SpriteRenderer>();
        gm = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void LateUpdate()
    {

    }

    public void PlayWalkAnim()
    {

    }

    public string GetNearestCompassDirectionToPlayer()
    {
        if (!useDirectionalAnimations)
            return "S";

        Vector3 lookAt = gm.globalCameraReference.transform.position - transform.position;
        float spriteAngle = Vector3.Angle(new Vector3(lookAt.x, 0, lookAt.z), body.transform.forward);
        int factor = 45;
        float dir = 360 * Mathf.Clamp01(AngleDir(body.transform.forward, lookAt, body.transform.up));
        int roundedAngle = Mathf.Abs((int)dir - Mathf.RoundToInt(spriteAngle / factor) * factor);

        string compass = "S";
        switch (roundedAngle)
        {
            case 45:
                compass = "SW";
                break;
            case 90:
                compass = "W";
                break;
            case 135:
                compass = "NW";
                break;
            case 180:
                compass = "N";
                break;
            case 225:
                compass = "NE";
                break;
            case 270:
                compass = "E";
                break;
            case 315:
                compass = "SE";
                break;
        }
        return compass;
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

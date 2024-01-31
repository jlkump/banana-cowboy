using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

/**
 * This is a component added to any planet, platform, or object to which we
 * want the player and other gravity objects to be attracted to. The direction
 * of gravity is determined by AttractionDirection and can be Radial (as it is
 * for planets) or in some local X / Y / Z direction relative to the gravity attractor.
 * 
 * For Radial attractors, the center of gravity is also chose as the direction to which
 * the object is attracted. This can be useful in the case of half-spheres or other
 * non-uniform spherical objects, as, by default, the center of gravity is
 * at the center of the object, which may not be what is wanted. (However,
 * in most cases, it will be).
 * 
 * A Trigger CollisionObject must be placed on the G
 */

public class GravityAttractor : MonoBehaviour
{

    public float gravity = -20.0f;
    public float gravityIncreaseOnFalling = 1.5f;
    public int priority = 0; // Higher priority overrides lower priority
    public enum AttractDirection
    {
        RADIAL,
        OBJECT_X,
        OBJECT_Y,
        OBJECT_Z
    }

    public AttractDirection attractDirection = AttractDirection.RADIAL;
    public Transform centerOfGravity; // Only really matters for radial

    public void Awake()
    {
        centerOfGravity = transform;
    }

    public void Attract(Transform body, Transform orientation, float maxFallSpeed, float gravityMult)
    {
        Vector3 targetDir = GetGravityUp(orientation);
        //Vector3 fallVec = body.InverseTransformDirection(body.GetComponent<CharacterController>());
        //fallVec.x = 0;
        //fallVec.z = 0;

        //if (fallVec.magnitude < maxFallSpeed)
        //{
        //    if (fallVec.y < 0)
        //    {
        //        // Here we are falling down, so we increase the strength of gravity slightly to make the fall faster
        //        body.GetComponent<Rigidbody>().AddForce(targetDir * gravity * gravityMult * gravityIncreaseOnFalling);
        //    }
        //    else
        //    {
        //        body.GetComponent<Rigidbody>().AddForce(targetDir * gravity * gravityMult);
        //    }
        //}
        var cc = body.GetComponent<CharacterController>();
        cc.Move(gravity * gravityMult * targetDir);
    }

    public int GetPriority()
    {
        return priority;
    }

    public void Reorient(Transform orientation, Transform model)
    {
        orientation.rotation = Quaternion.FromToRotation(orientation.up, GetGravityUp(orientation)) * orientation.rotation;
        Vector3 bodyUp = model.up;
        model.rotation = Quaternion.Slerp(model.rotation, Quaternion.FromToRotation(bodyUp, orientation.up) * model.rotation, Time.deltaTime * 6.0f);
    }

    Vector3 GetGravityUp(Transform body)
    {
        switch (attractDirection)
        {
            case AttractDirection.OBJECT_X:
                return transform.right;
            case AttractDirection.OBJECT_Y:
                return transform.up;
            case AttractDirection.OBJECT_Z:
                return transform.forward;
            case AttractDirection.RADIAL:
            default:
                return (body.position - centerOfGravity.position).normalized;
        }
    }
}
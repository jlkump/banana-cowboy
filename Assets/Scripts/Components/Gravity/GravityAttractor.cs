using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

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
[RequireComponent(typeof(Rigidbody))]
public class GravityAttractor : MonoBehaviour
{

    public float gravity = -20.0f;
    public int priority = 0; // Higher priority overrides lower priority
    public enum AttractDirection
    {
        RADIAL,
        OBJECT_X,
        OBJECT_Y,
        OBJECT_Z
    }

    public AttractDirection attractDirection = AttractDirection.RADIAL;
    public Transform centerOfGravity = null; // Only really matters for radial

    public void Awake()
    {
        if (centerOfGravity == null)
        {
            centerOfGravity = transform;
        }
    }

    public void Start()
    {
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    public Vector3 GetGravityDirection(Transform body)
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

    public float GetGravityForce()
    {
        return gravity;
    }

    public int GetPriority()
    {
        return priority;
    }
}
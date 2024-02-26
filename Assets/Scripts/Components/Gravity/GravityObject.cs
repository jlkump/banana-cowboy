using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * This component works in tandem with GravityAttractor
 * to attract the object to the associated attractor.
 * 
 * A GravityObject may be in the range of multiple
 * GravityAttractors, so the object is attracted to the
 * attractor with the highest priority.
 */
[RequireComponent(typeof(Rigidbody))]
public class GravityObject : MonoBehaviour
{
    Rigidbody _rigidBody;

    // The list of all attractors having influence on this object
    // This is looped through whenever the attractor list is updated
    // to find the highest priority attractor. The Gravity Object is
    // only attracted to the highest priority attractor.
    List<GravityAttractor> _attractors;
    int _highestPrioAttractorIndex = -1;

    // This determines terminal velocity to prevent gravity
    // from completely taking over and flinging things out into
    // the middle of nowhere
    public float maxFallSpeed { get; set; } = 30.0f;
    public bool disabled { get; set; } = false;

    // This is useful to modify when we want to have the player fall
    // at different speeds based on different situations. For example,
    // whenever the player lets go of the space button, we have them
    // fall faster than if they hold it down.
    public float gravityMult { get; set; } = 1.0f;

    [SerializeField, Range(1f, 10f)]
    float _gravityIncreaseOnFall = 1.5f;

    public Transform gravityOrientation = null;

    [SerializeField, Tooltip("Freeze rotation for the rigid-body")]
    bool _freezeRotation = true;

    GroundDetection _groundDetector;


    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        if (_freezeRotation)
        {
            _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        _attractors = new List<GravityAttractor>();

        if (gravityOrientation == null)
        {
            gravityOrientation = transform;
        }
        _groundDetector = GetComponentInChildren<GroundDetection>();
    }

    void FixedUpdate()
    {
        if (_highestPrioAttractorIndex != -1 && !_rigidBody.isKinematic && _groundDetector != null)
        {
            GravityAttractor attractor = _attractors[_highestPrioAttractorIndex];
            Vector3 targetGravUp = attractor.GetGravityDirection(gravityOrientation);

            // Reorient transform
            gravityOrientation.rotation = Quaternion.FromToRotation(gravityOrientation.up, targetGravUp) * gravityOrientation.rotation;


            // We are not on the ground yet, so pull to the nearest attractor
            Vector3 grav = attractor.GetGravityDirection(gravityOrientation) * attractor.GetGravityForce();
            Vector3 fallingVec = GetFallingVelocity();

            if (!_groundDetector.OnGround())
            {
                Debug.DrawRay(transform.position, grav, Color.yellow);
                if (fallingVec.magnitude < maxFallSpeed || Vector3.Dot(fallingVec, -targetGravUp) < 0.5f)
                {
                    if (gravityOrientation.InverseTransformDirection(fallingVec).y < 0)
                    {
                        // We are falling down, so increase gravity
                        _rigidBody.AddForce(_gravityIncreaseOnFall * gravityMult * grav);
                    }
                    else
                    {
                        _rigidBody.AddForce(gravityMult * grav);
                    }
                }
            }
            else
            {
                if (gravityOrientation.InverseTransformDirection(_rigidBody.velocity).y < 0)
                {
                    _rigidBody.velocity = GetMoveVelocity();
                }
            }
        }
    }

    int GetHighestPrioAttractorIndex()
    {
        int index = -1;
        int highest_prio = int.MinValue;
        for (int i = 0; i < _attractors.Count; i++)
        {
            if (_attractors[i].GetPriority() > highest_prio)
            {
                highest_prio = _attractors[i].GetPriority();
                index = i;
            }
        }
        return index;
    }

    /**
     * Get the vector for the direction the object is falling
     * in World-Space
     */
    public Vector3 GetFallingVelocity()
    {
        Vector3 vel = gravityOrientation.InverseTransformDirection(_rigidBody.velocity);
        vel.x = 0;
        vel.z = 0;
        return gravityOrientation.TransformDirection(vel);
    }

    /**
     * Get the vector for the direction the object is moving
     * in World-Space
     */
    public Vector3 GetMoveVelocity()
    {
        if (gravityOrientation == null || _rigidBody == null)
        {
            return Vector3.zero;
        }

        Vector3 vel = gravityOrientation.InverseTransformDirection(_rigidBody.velocity);
        vel.y = 0;
        return gravityOrientation.TransformDirection(vel);
    }

    public Vector3 GetGravityDirection()
    {
        if (_highestPrioAttractorIndex != -1)
        {
            return _attractors[_highestPrioAttractorIndex].GetGravityDirection(gravityOrientation);
        }
        return Vector3.zero;
    }

    public bool IsOnGround()
    {
        return _groundDetector.OnGround();
    }

    public bool IsInSpace()
    {
        return _attractors.Count <= 0;
    }

    // Whenever a Gravity Object enters one, we add the attractor to the list. Likewise,
    // whenever it leaves, we remove it from the list.
    void OnTriggerEnter(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponentInParent<GravityAttractor>() != null)
        {
            _attractors.Add(collision.gameObject.GetComponentInParent<GravityAttractor>());
            _highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
        }
    }

    void OnTriggerExit(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponentInParent<GravityAttractor>() != null)
        {
            _attractors.Remove(collision.gameObject.GetComponentInParent<GravityAttractor>());
            _highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
        }
    }
}
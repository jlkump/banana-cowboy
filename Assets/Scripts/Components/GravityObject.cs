using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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


    // This is useful to modify when we want to have the player fall
    // at different speeds based on different situations. For example,
    // whenever the player lets go of the space button, we have them
    // fall faster than if they hold it down.
    public float gravityMult { get; set; } = 1.0f;

    public float gravityIncreaseOnFall = 1.5f;

    public bool disabled { get; set; } = false;

    [Header("References")]
    [Tooltip("This is the reference to the transform of the root of the model")]
    public Transform model = null;
    [Tooltip("Turn this to false if the 3D model should not be re-oriented to the direction of gravity")]
    public bool reorientModel = true;

    public Transform gravityOrientation = null;
    PlayerCameraController _camController;

    [Header("Ground Detection")]
    // Used to determine what is and what is not ground.
    // It is a LayerMask and not a Tag incase we ever need to
    // Raycast to detect the ground (which we likely will for enemies).
    public LayerMask groundMask;
    public Transform bottomModelLocation = null;
    [Tooltip("This determines how far we look below bottomModelLocation for the ground")]
    public float heightDetection = 0.1f;
    public float heightDetectionRadius = 0.3f;

    // Updated whenever the Gravity Object collides with something. If that
    // something has the groundMask layer on it, then this is updated to
    // true (regardless if that ground was the ground the object is being attracted to.
    // This currently is problematic for edge cases, so TODO: Update to identify the
    // ground hit).
    // ^^^ Also, there will be cases that there are ground objects that don't have a
    //     gravity attractor component, so that is another case to keep in mind.
    private bool _onGround = false;

    Rigidbody _rigidBody;

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _rigidBody.useGravity = false;
        _rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
        _attractors = new List<GravityAttractor>();

        _camController = GetComponent<PlayerCameraController>();

        if (gravityOrientation == null)
        {
            gravityOrientation = transform;
        }
    }

    void FixedUpdate()
    {
        if (_highestPrioAttractorIndex != -1 && bottomModelLocation != null && !disabled && !_rigidBody.isKinematic)
        {
            GravityAttractor attractor = _attractors[_highestPrioAttractorIndex];
            RaycastHit hit;
            _onGround = Physics.SphereCast(bottomModelLocation.position, heightDetectionRadius, -gravityOrientation.up, out hit, heightDetection, groundMask, QueryTriggerInteraction.Ignore);
            Vector3 targetGravUp = attractor.GetGravityDirection(gravityOrientation);
            // Reorient transform
            if (model != null && reorientModel)
            {
                // Reorient model if we have one (and are not prevented from doing it)
                model.rotation = Quaternion.Slerp(model.rotation, Quaternion.FromToRotation(model.up, targetGravUp) * model.rotation, Time.deltaTime * 6.0f);
            }
            if (_camController != null && gravityOrientation.up != targetGravUp)
            {
                _camController.SetNewUp(targetGravUp);
            }
            gravityOrientation.rotation = Quaternion.FromToRotation(gravityOrientation.up, targetGravUp) * gravityOrientation.rotation;


            // We are not on the ground yet, so pull to the nearest attractor
            Vector3 grav = attractor.GetGravityDirection(gravityOrientation) * attractor.GetGravityForce();
            Vector3 fallingVec = GetFallingVelocity();
            if (!_onGround)
            {
                if (fallingVec.magnitude < maxFallSpeed)
                {
                    if (gravityOrientation.InverseTransformDirection(fallingVec).y < 0)
                    {
                        // We are falling down, so increase gravity
                        _rigidBody.AddForce(gravityIncreaseOnFall * gravityMult * grav);
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
        return _onGround;
    }

    public bool IsInSpace()
    {
        return _attractors.Count <= 0;
    }

    // Whenever a Gravity Object enters one, we add the attractor to the list. Likewise,
    // whenever it leaves, we remove it from the list.
    void OnTriggerEnter(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponent<GravityAttractor>() != null)
        {
            //print("Entered gravity attractor pull");
            _attractors.Add(collision.gameObject.GetComponent<GravityAttractor>());
        }
        _highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
    }

    void OnTriggerExit(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponent<GravityAttractor>() != null)
        {
            //print("Left gravity attractor pull");
            _attractors.Remove(collision.gameObject.GetComponent<GravityAttractor>());
        }
        _highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
    }
}
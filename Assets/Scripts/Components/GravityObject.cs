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
[RequireComponent(typeof(CharacterController))]
public class GravityObject : MonoBehaviour
{
    // The list of all attractors having influence on this object
    // This is looped through whenever the attractor list is updated
    // to find the highest priority attractor. The Gravity Object is
    // only attracted to the highest priority attractor.
    List<GravityAttractor> attractors;
    int highestPrioAttractorIndex = -1;


    // Used to determine what is and what is not ground.
    // It is a LayerMask and not a Tag incase we ever need to
    // Raycast to detect the ground (which we likely will for enemies).
    public LayerMask groundMask;

    // This determines terminal velocity to prevent gravity
    // from completely taking over and flinging things out into
    // the middle of nowhere
    public float maxFallSpeed { get; set; } = 30.0f;


    // This is useful to modify when we want to have the player fall
    // at different speeds based on different situations. For example,
    // whenever the player lets go of the space button, we have them
    // fall faster than if they hold it down.
    public float gravityMult { get; set; } = 1.0f;

    // Updated whenever the Gravity Object collides with something. If that
    // something has the groundMask layer on it, then this is updated to
    // true (regardless if that ground was the ground the object is being attracted to.
    // This currently is problematic for edge cases, so TODO: Update to identify the
    // ground hit).
    // ^^^ Also, there will be cases that there are ground objects that don't have a
    //     gravity attractor component, so that is another case to keep in mind.
    private bool _onGround = false;


    public Transform model = null;
    public Transform orientation = null;

    void Awake()
    {
        attractors = new List<GravityAttractor>();
    }

    void FixedUpdate()
    {
        if (highestPrioAttractorIndex != -1 && orientation != null && model != null)
        {
            GravityAttractor attractor = attractors[highestPrioAttractorIndex];
            attractor.Reorient(orientation, model);
            if (!_onGround)
            {
                attractor.Attract(transform, orientation, maxFallSpeed, gravityMult);
            }
        }
    }

    int GetHighestPrioAttractorIndex()
    {
        int index = -1;
        int highest_prio = int.MinValue;
        for (int i = 0; i < attractors.Count; i++)
        {
            if (attractors[i].GetPriority() > highest_prio)
            {
                highest_prio = attractors[i].GetPriority();
                index = i;
            }
        }

        return index;
    }

    // Trigger collision objects determine the range of influence for the GravityAttractor.
    // Whenever a Gravity Object enters one, we add the attractor to the list. Likewise,
    // whenever it leaves, we remove it from the list.
    void OnTriggerEnter(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponent<GravityAttractor>() != null)
        {
            attractors.Add(collision.gameObject.GetComponent<GravityAttractor>());
        }
        highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
    }

    void OnTriggerExit(UnityEngine.Collider collision)
    {
        if (collision != null && collision.gameObject != null && collision.gameObject.GetComponent<GravityAttractor>() != null)
        {
            attractors.Remove(collision.gameObject.GetComponent<GravityAttractor>());
        }
        highestPrioAttractorIndex = GetHighestPrioAttractorIndex();
    }


    // Collision Enter does not detect trigger collision objects, which makes it perfect for
    // detecting when the player hit the ground.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject != null &&
            (groundMask & 1 << collision.gameObject.layer) > 0 &&
            Vector3.Dot((orientation.position - collision.gameObject.transform.position), orientation.up) > -0.3)
        {
            print("On surface");
            _onGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision != null &&
            (groundMask & 1 << collision.gameObject.layer) > 0 &&
            Vector3.Dot((orientation.position - collision.gameObject.transform.position), orientation.up) > -0.3)
        {
            print("Left surface");
            _onGround = false;
        }
    }

    public bool IsOnGround()
    {
        return _onGround;
    }
}
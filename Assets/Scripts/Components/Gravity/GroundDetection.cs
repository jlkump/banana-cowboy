using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    private bool _onGround;

    private void OnTriggerStay(Collider other)
    {
        if ((transform.position - other.ClosestPoint(transform.position)).magnitude == 0 || 
            Vector3.Dot(transform.up, (transform.position - other.ClosestPoint(transform.position)).normalized) > 0.1f) {
            Debug.DrawLine(other.ClosestPoint(transform.position), transform.position, Color.green);
            _onGround = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _onGround = false;
    }

    public bool OnGround()
    {
        return _onGround;
    }
}

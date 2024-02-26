using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundDetection : MonoBehaviour
{
    private bool _onGround;

    private void OnCollisionEnter(Collision collision)
    {
        _onGround = true;
    }
    //private void OnCollisionStay(Collision collision)
    //{
    //    _onGround = true;
    //}

    private void OnTriggerStay(Collider other)
    {
        _onGround = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        _onGround = false;
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

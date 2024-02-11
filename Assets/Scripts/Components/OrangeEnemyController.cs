using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(GravityObject))]
public class OrangeEnemyController : EnemyController
{
    enum OrangeState
    {
        IDLE,
        ROAM,
        PLAYER_SPOTTED,
        REV_UP,
        CHARGE,
        RUN_AWAY,
        DIZZY
    }

    private OrangeState _state;

    public float dizzyTime = 1.0f;
    public float knockbackForce = 4.0f;
    public float chargeSpeed = 40.0f;
    public float maxChargeDistance = 70.0f;
    public float walkSpeed = 10.0f;

    private Transform _spottedPlayerTransform = null;
    private GravityObject _gravObject = null;
    private Vector3 _chargeStartPoint;
    private Vector3 _chargeDirection;

    void UpdateState(OrangeState newState)
    {
        _state = newState;
    }
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<LassoObject>().isLassoable = false;
        _gravObject = GetComponent<GravityObject>();
    }

    // Update is called once per frame
    void Update()
    {
        switch(_state)
        {
            case OrangeState.REV_UP:
                _chargeDirection = (_spottedPlayerTransform.position - transform.position);
                _gravObject.model.rotation = Quaternion.LookRotation(_chargeDirection, _gravObject.gravityOrientation.up);
                break;
            case OrangeState.PLAYER_SPOTTED: 

                break;
            case OrangeState.RUN_AWAY:

                break;
            case OrangeState.CHARGE: 

                break;
            case OrangeState.IDLE:
            default:
                break;
        }
    }
    void EndDizzy()
    {
        UpdateState(OrangeState.RUN_AWAY);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject != null)
        {
            if (collision.gameObject.GetComponent<Obstacle>() != null)
            {
                UpdateState(OrangeState.DIZZY);
                Invoke("EndDizzy", dizzyTime);
            }
            else if (collision.gameObject.GetComponent<PlayerController>() != null && _state == OrangeState.CHARGE)
            {
                collision.gameObject.GetComponent<PlayerController>().Damage(1, (collision.gameObject.transform.position - transform.position).normalized * knockbackForce);
                UpdateState(OrangeState.RUN_AWAY);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.gameObject.GetComponent<PlayerController>() != null)
            {
                _spottedPlayerTransform = other.gameObject.transform;
                UpdateState(OrangeState.REV_UP);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.gameObject.GetComponent<PlayerController>() != null)
            {
                _spottedPlayerTransform = null;
                UpdateState(OrangeState.IDLE);
            }
        }
    }
}

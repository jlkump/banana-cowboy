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
        SLOW_DOWN,
        RUN_AWAY,
        DIZZY,
        HELD,
        THROWN
    }

    private OrangeState _state;

    public float knockbackForce = 4.0f;
    public float chargeSpeed = 30.0f;
    public float maxChargeDistance = 70.0f;
    public float walkSpeed = 10.0f;

    public float timeToBeginRev = 0.4f;
    public float timeToBeginCharge = 0.3f;
    public float timeSpentCharging = 0.8f;
    public float dizzyTime = 1.0f;
    public float timeSpentRunningAway = 0.4f;

    private float spottedParam = 0.0f;

    private Transform _spottedPlayerTransform = null;
    private GravityObject _gravObject = null;
    private LassoableEnemy _lassoComp = null;
    private Vector3 _chargeStartPoint;
    private Vector3 _chargeDirection;
    private Vector3 _chargeTargetPoint;
    // Start is called before the first frame update
    void Start()
    {
        _lassoComp = GetComponent<LassoableEnemy>();
        _lassoComp.isLassoable = false;
        _gravObject = GetComponent<GravityObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_lassoComp.currentlyLassoed) { UpdateState(OrangeState.HELD); }
        if (_state == OrangeState.HELD) {
            if (!_lassoComp.currentlyLassoed)
            {
                UpdateState(OrangeState.THROWN);
            }
            return; 
        }

        if ((_state == OrangeState.PLAYER_SPOTTED || _state == OrangeState.REV_UP) && _spottedPlayerTransform == null)
        {
            UpdateState(OrangeState.IDLE);
        }
        switch(_state)
        {
            case OrangeState.PLAYER_SPOTTED:
                spottedParam += Time.deltaTime;
                _chargeDirection = (_spottedPlayerTransform.position - transform.position).normalized;
                _gravObject.model.rotation = Quaternion.Slerp(
                    _gravObject.model.rotation, 
                    Quaternion.LookRotation(_chargeDirection, _gravObject.gravityOrientation.up), 
                    spottedParam
                );
                break;
            case OrangeState.REV_UP:
                _chargeDirection = (_spottedPlayerTransform.position - transform.position).normalized;
                _gravObject.model.rotation = Quaternion.LookRotation(_chargeDirection, _gravObject.gravityOrientation.up);
                break;
            case OrangeState.CHARGE: 
                if (_gravObject.GetMoveVelocity().magnitude < chargeSpeed)
                {
                    GetComponent<Rigidbody>().AddForce(_chargeDirection * chargeSpeed);
                }
                break;
            case OrangeState.RUN_AWAY:
                // TODO
                break;
            case OrangeState.IDLE:
            default:

                break;
        }
    }

    void UpdateState(OrangeState newState)
    {
        if (_state != newState)
        {
            UpdateAnimState();

            switch (newState)
            {
                case OrangeState.PLAYER_SPOTTED:
                    spottedParam = 0.0f;
                    Invoke("EndPlayerSpotted", timeToBeginRev);
                    break;
                case OrangeState.REV_UP:
                    _chargeStartPoint = transform.position;
                    Invoke("EndRevUp", timeToBeginCharge);
                    break;
                case OrangeState.CHARGE:
                    Invoke("EndCharge", timeSpentCharging);
                    break;
                case OrangeState.RUN_AWAY:
                    Invoke("EndRunAway", timeSpentRunningAway);
                    break;
                case OrangeState.SLOW_DOWN:
                    Invoke("EndSlowDown", timeSpentRunningAway);
                    break;
                case OrangeState.DIZZY:
                    Invoke("EndDizzy", dizzyTime);
                    break;
                case OrangeState.IDLE:
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    break;
                case OrangeState.ROAM:
                    break;
                case OrangeState.HELD:
                    break;
            }
            if (newState == OrangeState.DIZZY)
            {
                _lassoComp.isLassoable = true;
            }
            else
            {
                _lassoComp.isLassoable = false;
            }
        }
        _state = newState;
    }

    void UpdateAnimState()
    {

    }


    void EndPlayerSpotted()
    {
        UpdateState(OrangeState.REV_UP);
    }

    void EndRevUp()
    {
        UpdateState(OrangeState.CHARGE);
    }

    void EndCharge()
    {
        if (_state != OrangeState.CHARGE) { return; }
        UpdateState(OrangeState.SLOW_DOWN);
    }

    void EndSlowDown()
    {
        UpdateState(OrangeState.RUN_AWAY);
    }

    void EndRunAway()
    {
        if (_spottedPlayerTransform != null)
        {
            UpdateState(OrangeState.PLAYER_SPOTTED);
        }
        else
        {
            UpdateState(OrangeState.IDLE);
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
                collision.gameObject.GetComponent<Obstacle>().Hit();
                UpdateState(OrangeState.DIZZY);
            }
            else if (collision.gameObject.tag == "Player" && _state == OrangeState.CHARGE)
            {
                collision.gameObject.GetComponentInParent<PlayerController>().Damage(1, (collision.gameObject.transform.position - transform.position).normalized * knockbackForce);
                UpdateState(OrangeState.RUN_AWAY);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.gameObject.tag == "Player")
            {
                _spottedPlayerTransform = other.gameObject.transform;
                UpdateState(OrangeState.PLAYER_SPOTTED);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.gameObject.tag == "Player")
            {
                // Might change it so that the player has to run further than the trigger collider to leave sight once spotted
                _spottedPlayerTransform = null;
                UpdateState(OrangeState.IDLE);
            }
        }
    }
}

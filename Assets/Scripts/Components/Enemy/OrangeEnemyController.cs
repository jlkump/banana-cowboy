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
        DIZZY,
        HELD,
        THROWN
    }

    private OrangeState _state;

    [SerializeField]
    Transform _model = null;

    public float knockbackForce = 4.0f;
    public float chargeSpeed = 30.0f;
    public float maxChargeDistance = 70.0f;
    public float walkSpeed = 10.0f;

    public float timeToBeginRev = 0.4f;
    public float timeToBeginCharge = 0.3f;
    public float timeSpentCharging = 0.8f;
    public float dizzyTime = 1.0f;

    private float spottedParam = 0.0f;

    private Transform _spottedPlayerTransform = null;
    private GravityObject _gravObject = null;
    private LassoableEnemy _lassoComp = null;
    private Vector3 _chargeStartPoint;
    private Vector3 _chargeDirection;
    private Vector3 _chargeTargetPoint;

    public bool playerInView = false;

    public Animator orangeEnemyAnimator;
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
        if (_state == OrangeState.HELD)
        {
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
        switch (_state)
        {
            case OrangeState.PLAYER_SPOTTED:
                spottedParam += Time.deltaTime;
                _chargeDirection = (_spottedPlayerTransform.position - transform.position).normalized;
                if (_model != null)
                {
                    _model.rotation = Quaternion.Slerp(_model.rotation,
                        Quaternion.LookRotation(_chargeDirection, _gravObject.gravityOrientation.up),
                        spottedParam);
                }
                break;
            case OrangeState.REV_UP:
                _chargeDirection = (_spottedPlayerTransform.position - transform.position).normalized;
                if (_model != null)
                {
                    _model.rotation = Quaternion.LookRotation(_chargeDirection, _gravObject.gravityOrientation.up);
                }
                break;
            case OrangeState.CHARGE:
                if (_gravObject.GetMoveVelocity().magnitude < chargeSpeed)
                {
                    GetComponent<Rigidbody>().AddForce(_chargeDirection * chargeSpeed);
                }
                break;
            case OrangeState.IDLE:
            default:

                break;
        }
    }

    void UpdateState(OrangeState newState)
    {
        if (_state == OrangeState.HELD && newState != OrangeState.THROWN) { return; }
        if (_state != newState)
        {
            _state = newState;
            UpdateAnimState();

            switch (newState)
            {
                case OrangeState.PLAYER_SPOTTED:
                    spottedParam = 0.0f;
                    Invoke("EndPlayerSpotted", timeToBeginRev);
                    break;
                case OrangeState.REV_UP:
                    _chargeStartPoint = transform.position;
                    SoundManager.Instance().PlaySFX("OrangeRevUp");
                    Invoke("EndRevUp", timeToBeginCharge);
                    break;
                case OrangeState.CHARGE:
                    SoundManager.Instance().StopSFX("OrangeRevUp");
                    SoundManager.Instance().PlaySFX("OrangeCharge");
                    Invoke("EndCharge", timeSpentCharging);
                    break;
                case OrangeState.DIZZY:
                    SoundManager.Instance().PlaySFX("OrangeDizzy");
                    Invoke("EndDizzy", dizzyTime);
                    break;
                case OrangeState.IDLE:
                    GetComponent<Rigidbody>().velocity = Vector3.zero;
                    break;
                case OrangeState.THROWN:
                    break;
                case OrangeState.HELD:
                    SoundManager.Instance().StopSFX("OrangeDizzy");
                    break;
            }
            if (newState == OrangeState.DIZZY || newState == OrangeState.HELD)
            {
                print("Lassoable");
                _lassoComp.isLassoable = true;
            }
            else
            {
                print("Not lassoable");
                _lassoComp.isLassoable = false;
            }
        }
    }

    void UpdateAnimState()
    {
        if (orangeEnemyAnimator == null) { return; }
        if (_state == OrangeState.IDLE)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Idle");
            print("IDLE");
        }
        if (_state == OrangeState.PLAYER_SPOTTED)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Player_Spotted");
            print("PLAYER SPOTTED");
        }
        if (_state == OrangeState.CHARGE)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Roll_Anticipation");
            print("CHARGE");
        }
        if (_state == OrangeState.DIZZY)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Stun");
            print("DIZZY");
        }
        if (_state == OrangeState.HELD)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Lassoed");
            print("HELD");
        }
        if (_state == OrangeState.REV_UP)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Rev_Up"); // CHANGE
            print("REV UP");
        }
        if (_state == OrangeState.ROAM)
        {
            print("ROAM");
        }
        if (_state == OrangeState.SLOW_DOWN)
        {
            print("SLOW DOWN");
        }
        if (_state == OrangeState.THROWN)
        {
            orangeEnemyAnimator.Play("Base Layer.OE_Thrown");
            print("THROWN");
        }
    }


    void EndPlayerSpotted()
    {
        if (_state != OrangeState.PLAYER_SPOTTED) { return; }
        UpdateState(OrangeState.REV_UP);
    }

    void EndRevUp()
    {
        if (_state != OrangeState.REV_UP) { return; }
        UpdateState(OrangeState.CHARGE);
    }

    void EndCharge()
    {
        if (_state != OrangeState.CHARGE) { return; }
        UpdateState(OrangeState.SLOW_DOWN);
    }

    void EndDizzy()
    {
        if (_state != OrangeState.DIZZY || _state != OrangeState.THROWN) { return; }
        SoundManager.Instance().StopSFX("OrangeDizzy");
        if (playerInView)
        {
            UpdateState(OrangeState.PLAYER_SPOTTED);
        }
        else
        {
            _spottedPlayerTransform = null;
            UpdateState(OrangeState.IDLE); 
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && collision.gameObject != null)
        {
            if (collision.gameObject.GetComponent<Obstacle>() != null && _state != OrangeState.THROWN)
            {
                collision.gameObject.GetComponent<Obstacle>().Hit();
                SoundManager.Instance().StopSFX("OrangeCharge");
                SoundManager.Instance().PlaySFX("OrangeHitWall");
                UpdateState(OrangeState.DIZZY);
            }
            else if (collision.gameObject.tag == "Player" && _state == OrangeState.CHARGE)
            {
                collision.gameObject.GetComponentInParent<PlayerController>().Damage(1, (collision.gameObject.transform.position - transform.position).normalized * knockbackForce);
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
                playerInView = true;
                // was a bug where when a player jumps while enemy is charging (escapes the triggerbox), then enters again
                // if the player gets hit, would not take damage.
                if (_state != OrangeState.CHARGE && _state != OrangeState.DIZZY)
                {
                    UpdateState(OrangeState.PLAYER_SPOTTED);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other != null && other.gameObject != null)
        {
            if (other.gameObject.tag == "Player")
            {
                playerInView = false;
            }
        }
    }
}

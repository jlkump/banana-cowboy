using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Device;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(GravityObject))]
public class PlayerController : MonoBehaviour
{
    [Header("Debug")]


    [Header("References")]
    public Transform model;
    public Transform emptyObjectPrefab;
    // We can find these ourselves
    Rigidbody _rigidBody;
    Transform _cameraTransform;
    GravityObject _gravityObject;


    [Header("Movement")]
    [Tooltip("The maximum walk speed.")]
    public float walkSpeed = 8.0f;
    [Tooltip("The maximum run speed.")]
    public float runSpeed = 12.0f;

    [Tooltip("The rate of speed increase for getting to max walk / run speed")]
    public float accelerationRate = (50f * 0.5f) / 8.0f;  // The rate of speed increase
    [Tooltip("The rate of speed decrease when slowing the player down to no movement input")]
    public float deccelerationRate = (50f * 0.5f) / 8.0f; // The rate of speed decrease (when no input is pressed)
    [Tooltip("The ratio for the player to control the player while in the air relative to ground movement. Scale [0, 1.0]. 0.5 means 50% the effective acceleration in the air relative to the ground.")]
    public float accelAirControlRatio = 0.8f;       // The ability for the player to re-orient themselves in the air while accelerating
    [Tooltip("The ratio for the player to control the player while in the air relative to ground movement, same as Accel Air Control Ratio, but how much is lost when the player is slowing down in the air.")]
    public float deccelAirControlRatio = 0.8f;      // The ability for the player to move themselves while in the air and deccelerating (range [0.0,1.0])
    public float jumpImpulseForce = 10.0f;
    [Tooltip("Determines how much the force of gravity is increased when the jump key is released.")]
    public float gravIncreaseOnJumpRelease = 3.0f;
    [Tooltip("If the player somehow achieves speed greater than the maximum allowed, we won't give them any more speed. However, with conserve momentum, we won't reduce their speed either.")]
    public bool conserveMomentum = true;

    private Vector3 _moveInput;
    private float _lastTimeOnGround = 0.0f;
    private bool _isRunning = false;
    
    // This tracks the velocity of the player's lateral movement at the time of jumping so we restore it when landing
    private Vector3 _jumpMoveVelocity; 
    private bool _detectLanding = false;

    [Header("Lasso")]
    public Transform lassoThrowPosition;
    public float lassoRange = 15.0f;
    public float lassoAimAssistRadius = 1.0f;
    public Transform lassoPredictionReticule;
    public LayerMask lassoLayerMask;
    public LineRenderer lassoLineRenderer;

    private RaycastHit _lassoRaycastHit;
    private Vector3 _lassoHitPoint;
    //private bool _aimingLasso = false;

    [Header("Swinging")]
    public float endSwingBoostForce = 5.0f;
    public float endSwingVerticalBoostForce = 2.0f;
    public float minSwingRadius = 3.0f;
    public float maxSwingRadius = 10.0f;
    public float distanceOfSwing = 0.9f * Mathf.PI; // On the range [0, 2PI]. The amount we cover the circumfrence of the circle.
    [Tooltip("The maximum swinging speed.")]
    public float maxSwingSpeed = 3.0f;
    public float startingSwingSpeed = 1.2f;
    public float swingAccelRate = 0.4f;
    public float swingSideRotateSpeed = 1f;

    Transform _swingTransform;
    private Vector3 _swingPosition;
    private float _swingRadius;
    private float _swingVelocity;
    private float _swingProgress;

    [Header("Input")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode lassoKey = KeyCode.Mouse0;
    //public KeyCode aimLassoKey = KeyCode.Mouse1;

    /*
    [Header("Buffer System")]
    public float jump_hold_buffer = 0.3f;
    private float jump_hold_buffer_timer;
    public float jump_buffer = 0.1f;
    private float jump_buffer_timer;

    // Having as serialized field messes up pause menu
    public GameManager gameManager;

    [Header("Sound Effects")]
    public SoundManager soundManager;
    public AudioClip walkSFX;
    public AudioClip runSFX;
    public AudioClip jumpSFX;
    public AudioClip dashSFX;
    public AudioClip ropeThrowSFX;
    public AudioClip ropeWindUpSFX;
    public AudioClip ropeSpinningEnemySFX;
    public AudioClip landingSFX;
    */
    enum PlayerState
    {
        IDLE,
        AIR,
        WALK,
        RUN,
        SWING,
    };

    private PlayerState _state = PlayerState.AIR;
    

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _gravityObject = GetComponent<GravityObject>();
        lassoLineRenderer.positionCount = 0;
    }

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _swingTransform = Instantiate(emptyObjectPrefab, transform.position, Quaternion.identity).transform;
    }
    void Update()
    {
        // Later, we will need to ignore input to the player controller
        // when in the UI or when in cutscenes
        if (true)
        {
            if (_state != PlayerState.SWING)
            {
                GetMoveInput();
                AimLasso();
                GetLassoInput();
            } 
            else
            {
                GetSwingInput();
            }
        }
    }

    void FixedUpdate()
    {
        if (_state != PlayerState.SWING)
        {
            if (!_gravityObject.IsInSpace())
            {
                Run();
            }
        } 
        else
        {
            Swing();
        }
    }
    private void LateUpdate()
    {
        if (_state == PlayerState.SWING)
        {
            DrawRope();
        }
    }

    void UpdateState(PlayerState newState)
    {
        _state = newState;
        if (_state != newState)
        {
            // Signal update to animation
            UpdateAnimState();
            if (_state == PlayerState.AIR)
            {
                // Whenever we enter the AIR state, we should detect the next time we land on the ground.
                _detectLanding = true;
            }
        }
    }

    void UpdateAnimState()
    {
        // This handles the changes to the animimations based on the player state
    }

    bool IsValidState(PlayerState newState)
    {
        return true;
    }

    void GetMoveInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical).normalized;

        if (Input.GetKeyDown(sprintKey))
        {
            _isRunning = true;
        } 
        else if (Input.GetKeyUp(sprintKey))
        {
            _isRunning = false;
        }

        // _lastTimeOnGround is used to see whenever the player is considered "in the air"
        // for movement controls. When in the air, the player has drag on their movement
        // determined by accelAirControlRatio and deccelAirControlRatio.
        // When on the ground, _lastTimeOnGround acts as a coyote timer when
        // we still accept jump input.
        _lastTimeOnGround -= Time.deltaTime;
        if (_gravityObject.IsOnGround())
        {
            if (_detectLanding)
            {
                // This is how we detect if the player has landed or not.
                // It is called once, only on landing after falling off a ledge or jumping.
                OnLand();
            }
            _lastTimeOnGround = 0.1f; // This might be good to later have a customizable parameter, but 0.1f seems good for now.
            if (_moveInput == Vector3.zero)
            {
                UpdateState(PlayerState.IDLE);
            } 
            else if (_isRunning)
            {
                // Important to note, checking isRunning only works here b/c we first check that _moveInput is non-zero
                // otherwise the player could change to the running state by simply pressing the runningKey. If logic
                // changes here, make sure this can not happen.
                UpdateState(PlayerState.RUN);
            } 
            else
            {
                UpdateState(PlayerState.WALK);
            }
        } 
        else if (_lastTimeOnGround <= 0)
        {
            // We are no longer on the ground, change state to air
            UpdateState(PlayerState.AIR);
        }

        if (Input.GetKeyDown(jumpKey)) 
        {
            // Last time on ground acts as a coyote timer for jumping
            if (_lastTimeOnGround > 0)
            {
                StartJump();
            }
            else if (false)
            {
                // Here we can add logic for jump buffering
            }
        } 
        else if (false)
        {
            // Again, this is here for jump buffering
        }
        else if (Input.GetKeyUp(jumpKey))
        {
            EndJump();
        }
    }

    /**
     * Code for running momentum used from https://github.com/DawnosaurDev/platformer-movement/blob/main/Scripts/Run%20Only/PlayerRun.cs#L79
     */
    void Run()
    {
        // Transform the move input relative to the camera
        _moveInput = _cameraTransform.TransformDirection(_moveInput);
        // Transform the move input relative to the player
        _moveInput = Vector3.Dot(transform.right, _moveInput) * transform.right + Vector3.Dot(transform.forward, _moveInput) * transform.forward;
        Vector3 targetVelocity = _moveInput * walkSpeed;
        float targetSpeed = targetVelocity.magnitude;

        float accelRate;
        // Gets an acceleration value based on if we are accelerating (includes turning) 
        // or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (_lastTimeOnGround > 0)
            accelRate = (Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0) ? accelerationRate : deccelerationRate;
        else
            accelRate = (Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0) ? accelerationRate * accelAirControlRatio : deccelerationRate * deccelAirControlRatio;


        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (conserveMomentum &&
            Mathf.Abs(_gravityObject.GetMoveVelocity().magnitude) > Mathf.Abs(targetSpeed) &&
            Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0 &&
            Mathf.Abs(targetSpeed) > 0.01f && _lastTimeOnGround < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }

        Vector3 speedDiff = targetVelocity - _gravityObject.GetMoveVelocity();
        Vector3 movement = speedDiff * accelRate;
        _rigidBody.AddForce(movement);

        // Spin player model and orientation to right direction to face
        if (_moveInput.magnitude > 0 && model != null)
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(targetVelocity.normalized, transform.up), Time.deltaTime * 8);
        }
    }

    void GetLassoInput()
    {
        //if (Input.GetKeyDown(aimLassoKey) && _state != PlayerState.SWING) { _aimingLasso = true; }
        //if (Input.GetKeyUp(aimLassoKey)) { _aimingLasso = false; }
        if (Input.GetKeyDown(lassoKey)) {
            if (_lassoRaycastHit.point != Vector3.zero)
            {
                if (_lassoRaycastHit.collider.gameObject.GetComponent<SwingableObject>() != null)
                {
                    StartSwing();
                }
            }
        }

    }

    void GetSwingInput()
    {

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical).normalized; // Re-using _moveInput, cause why not

        if (Input.GetKeyUp(lassoKey)) { EndSwing(); }
    }

    void AimLasso()
    {
        // Aim prediction from the following video:
        // https://www.youtube.com/watch?v=HPjuTK91MA8&list=PLh9SS5jRVLAleXEcDTWxBF39UjyrFc6Nb&index=15
        RaycastHit sphereCastHit;
        Physics.SphereCast(_cameraTransform.position, lassoAimAssistRadius, _cameraTransform.forward, 
            out sphereCastHit, lassoRange, lassoLayerMask, QueryTriggerInteraction.Ignore);

        RaycastHit raycastHit;
        Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, 
            out raycastHit, lassoRange, lassoLayerMask, QueryTriggerInteraction.Ignore);

        Vector3 hitPoint;

        if (raycastHit.point != Vector3.zero)
        {
            hitPoint = raycastHit.point;
        } 
        else if (sphereCastHit.point != Vector3.zero)
        {
            hitPoint = sphereCastHit.point;
        } 
        else
        {
            hitPoint = Vector3.zero;
        }

        if (hitPoint != Vector3.zero)
        {
            lassoPredictionReticule.gameObject.SetActive(true);
            lassoPredictionReticule.position = hitPoint;
        } 
        else
        {
            lassoPredictionReticule.gameObject.SetActive(false);
        }

        _lassoRaycastHit = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
    }

    void StartSwing()
    {
        Vector3 dirToPlayer = (transform.position - _lassoRaycastHit.point).normalized;
        Vector3 axisOfSwing = Vector3.Cross(_cameraTransform.transform.forward, dirToPlayer);
        if (axisOfSwing.magnitude == 0)
        {
            return;
        }
        UpdateState(PlayerState.SWING);

        _rigidBody.isKinematic = true; // Allows us to directly control the player's position so we can move them in a perfect arc
        _gravityObject.disabled = true;

        _swingTransform.position = _lassoRaycastHit.point;
        _swingPosition = _lassoRaycastHit.point;

        _swingRadius = Mathf.Clamp(Vector3.Distance(transform.position, _swingPosition), minSwingRadius, maxSwingRadius);
        _swingTransform.rotation = Quaternion.FromToRotation(_swingTransform.right, dirToPlayer) * _swingTransform.rotation;
        _swingTransform.rotation = Quaternion.FromToRotation(_swingTransform.up, axisOfSwing) * _swingTransform.rotation;

        lassoLineRenderer.positionCount = 2;

        _swingProgress = 0.0f;
        _swingVelocity = startingSwingSpeed;
    }

    void Swing()
    {
        // Move along a parametric curve
        // For now, lock the player right where they need to be.
        Vector3 prev = transform.position;
        transform.position = _swingPosition
                + _swingRadius * Mathf.Cos(_swingProgress) * _swingTransform.right
                + _swingRadius * Mathf.Sin(_swingProgress) * _swingTransform.forward;
        _swingProgress += _swingVelocity * Time.deltaTime;

        if (_swingProgress > distanceOfSwing)
        {
            EndSwing();
        }
        
        // Move input
        _swingVelocity = Mathf.Clamp(_swingVelocity + swingAccelRate * _moveInput.z, startingSwingSpeed, maxSwingSpeed);
        if (_moveInput.x > 0)
        {
            _swingTransform.Rotate(new Vector3(-swingSideRotateSpeed, 0, 0), Space.Self);
        } 
        else if (_moveInput.x < 0)
        {
            _swingTransform.Rotate(new Vector3(swingSideRotateSpeed, 0, 0), Space.Self);
        }

        // Re-orienting model
        Vector3 dirToPoint = (_swingPosition - transform.position).normalized;
        Vector3 newModelForward = (transform.position - prev).normalized;

        if (newModelForward.magnitude > 0)
        {
            model.rotation = Quaternion.FromToRotation(model.up, dirToPoint) * model.rotation;
            model.rotation = Quaternion.FromToRotation(model.forward, newModelForward) * model.rotation;
        }
        if (!_gravityObject.IsInSpace())
        {
            model.rotation = Quaternion.Slerp(
                Quaternion.FromToRotation(model.up, _gravityObject.GetGravityDirection()) * model.rotation, 
                model.rotation, 
                0.5f
            );
        }

    }

    void EndSwing()
    {
        UpdateState(PlayerState.AIR);

        _rigidBody.isKinematic = false;
        _gravityObject.disabled = false;

        lassoLineRenderer.positionCount = 0;

        _rigidBody.AddForce(endSwingVerticalBoostForce * transform.up + model.transform.forward * endSwingBoostForce * (_swingVelocity / maxSwingSpeed), ForceMode.Impulse);
    }

    void DrawRope()
    {
        lassoLineRenderer.SetPosition(0, lassoThrowPosition.position);
        lassoLineRenderer.SetPosition(1, _swingPosition);
    }

    void StartJump()
    {
        _rigidBody.AddForce(jumpImpulseForce * transform.up, ForceMode.Impulse);
        _gravityObject.gravityMult = 1.0f;
    }

    void EndJump()
    {
        print("Jump end");
        _gravityObject.gravityMult = gravIncreaseOnJumpRelease;
    }

    void OnLand()
    {
        print("Landed!");
        _detectLanding = false;
        //_aimingLasso = false;
    }
}

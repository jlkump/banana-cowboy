using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
    // We can find these ourselves
    Rigidbody _rigidBody;
    Transform _cameraTransform;
    GravityObject _gravityObject;


    [Header("Movement")]
    [Tooltip("The maximum walk speed.")]
    public float walkSpeed = 8.0f;
    [Tooltip("The maximum run speed.")]
    public float runSpeed = 12.0f;
    [Tooltip("The maximum swinging speed.")]
    public float swingSpeed = 15.0f;

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
    private bool _aimingLasso = false;

    [Header("Swinging")]
    public Transform swingOrientation;
    public float horizontalSwingForce = 2.0f;
    public float forwardSwingForce = 2.0f;
    public float extendSpeed = 2.0f;
    private SpringJoint _swingJoint;
    private Vector3 _swingPosition;


    [Header("Input")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode lassoKey = KeyCode.Mouse0;
    public KeyCode aimLassoKey = KeyCode.Mouse1;
    public KeyCode lassoReelIn = KeyCode.Space;
    public KeyCode lassoRollOut = KeyCode.LeftShift;

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
                GetLassoInput();
            } 
            else
            {
                GetSwingInput();
            }
            
            if (_aimingLasso)
            {
                AimLasso();
            }
        }
    }

    void FixedUpdate()
    {
        if (_state != PlayerState.SWING)
        {
            Run();
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
        if (Input.GetKeyDown(aimLassoKey) && _state != PlayerState.SWING) { _aimingLasso = true; }
        if (Input.GetKeyUp(aimLassoKey)) { _aimingLasso = false; }
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
        float forward = Input.GetAxisRaw("Vertical");

        _rigidBody.AddForce(horizontal * horizontalSwingForce * transform.TransformDirection(_cameraTransform.right).normalized);
        if (forward > 0)
        {
            _rigidBody.AddForce(forward * forwardSwingForce * transform.TransformDirection(_cameraTransform.forward).normalized);
        }

        if (Input.GetKey(lassoReelIn))
        {
            Vector3 directionToPoint = (_swingPosition - transform.position).normalized;
            _rigidBody.AddForce(directionToPoint * forwardSwingForce);

            float distanceFromPoint = Vector3.Distance(transform.position, _swingPosition);

            _swingJoint.maxDistance = distanceFromPoint * 0.8f;
            _swingJoint.minDistance = distanceFromPoint * 0.2f;
        }

        if (Input.GetKey(lassoRollOut))
        {
            float distanceFromPoint = Mathf.Clamp(Vector3.Distance(transform.position, _swingPosition) + extendSpeed, 0.2f, lassoRange);

            _swingJoint.maxDistance = distanceFromPoint * 0.8f;
            _swingJoint.minDistance = distanceFromPoint * 0.2f;
        }

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
        UpdateState(PlayerState.SWING);

        _swingPosition = _lassoRaycastHit.point;
        _swingJoint = gameObject.AddComponent<SpringJoint>();
        _swingJoint.autoConfigureConnectedAnchor = false;
        _swingJoint.connectedAnchor = _swingPosition;

        float distanceFromPoint = Vector3.Distance(transform.position, _swingPosition);

        _swingJoint.maxDistance = distanceFromPoint * 0.8f;
        _swingJoint.minDistance = distanceFromPoint * 0.2f;

        _swingJoint.spring = 4.5f;
        _swingJoint.damper = 7.0f;
        _swingJoint.massScale = 4.5f;

        lassoLineRenderer.positionCount = 2;
    }

    void EndSwing()
    {
        UpdateState(PlayerState.AIR);
        lassoLineRenderer.positionCount = 0;
        Destroy(_swingJoint);
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
        _aimingLasso = false;
    }

    /**
     * I think we decided not to have dash for now, so I've move the logic we had before
     * down here in case we decide to add it back later.
     */

    /*
    void GetDashInput()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse1) && can_Dash)
        {
            Dash();
            can_Dash = false;
            dash_Timer = dash_Cooldown;
        }

        if (!can_Dash)
        {
            dash_Timer -= Time.deltaTime;
            if (dash_Timer <= 0.0f)
            {
                can_Dash = true;
            }
        }
         
    }

    void Dash()
    {
        
        //GetComponent<Rigidbody>().AddForce(temp.transform.forward * dash_force, ForceMode.Impulse);
        soundManager.PlaySFX(dashSFX, 1);
        GetComponent<Rigidbody>().AddForce(modelTransform.forward * dash_force, ForceMode.Impulse);
        
    }
    */
}

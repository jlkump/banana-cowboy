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
using UnityEngine.UIElements;

[RequireComponent(typeof(GravityObject))]
public class PlayerController : MonoBehaviour
{
    [Header("Debug")]


    [Header("References")]
    public Transform model;
    public Transform emptyObjectPrefab;
    public Animator playerAnimator = null;
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
    public float lassoRange = 10.0f;
    [Tooltip("Determines the radius of the smallest sphere cast from the cursor pos. Must be less than lassoAimAssistMaxRadius")]
    public float lassoAimAssistMinRadius = 1.0f;
    [Tooltip("Determines the radius of the smallest sphere cast from the cursor pos. Must be greater than lassoAimAssistMinRadius")]
    public float lassoAimAssistMaxRadius = 15.0f;
    [Tooltip("The number of sphere-cast iterations for aim assist. Higher number means more fine grain aim assist. Must be at least 2")]
    public int lassoAimAssistNumIter = 5;
    public float lassoTimeToHit = 0.3f;
    public Transform lassoPredictionReticule;
    public LayerMask lassoLayerMask;
    public int numberOfLassoLoopSegments = 15;
    public int numberOfLassoRopeSegments = 15;
    public float lassoRadius = 0.3f;
    public float lassoWiggleSpeed = 20.0f;
    public float lassoWiggleAmplitude = 1.5f;

    private LassoRenderer _lassoRenderer;
    // This is NOT the transform of the object hit, but the transform of the specific raycast hit
    // For swinging, it is used for up calculating the basis of the swing for swinging
    // For enemies ...
    // For objects ...
    private Transform _lassoHitTransform;
    private Transform _hitObjectTransform; // The transform of the object actually hit
    private RaycastHit _lassoRaycastHit;
    private HitLassoTarget _hitLassoTarget;
    private bool _cancelLassoThrow;
    enum HitLassoTarget
    {
        NONE,
        SWINGABLE,
        ENEMY,
        OBJECT
    }

    [Header("Swinging")]
    public float endSwingBoostForce = 5.0f;
    public float endSwingVerticalBoostForce = 2.0f;
    public float minSwingRadius = 3.0f;
    public float maxSwingRadius = 10.0f;
    public float distanceOfSwing = 0.9f * Mathf.PI; // On the range [0, 2PI]. The amount we cover the circumfrence of the arc.
    [Tooltip("The maximum swinging speed.")]
    public float maxSwingSpeed = 3.0f;
    public float startingSwingSpeed = 1.2f;
    public float swingAccelRate = 0.4f;
    public float swingSideRotateSpeed = 1f;

    private float _swingRadius;
    private float _swingVelocity;
    private float _swingProgress;

    [Header("Input")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode lassoKey = KeyCode.Mouse0;
    
    enum PlayerState
    {
        IDLE,
        AIR,
        WALK,
        RUN,
        THROWING_LASSO,
        SWING,
        THROWING_ENEMY,
        THROWING_OBJECT
    };

    private PlayerState _state = PlayerState.AIR;
    private bool _heldObjectOrEnemy = false;
    

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _gravityObject = GetComponent<GravityObject>();

        if (playerAnimator != null)
        {
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
    }

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        _lassoHitTransform = Instantiate(emptyObjectPrefab, transform.position, Quaternion.identity).transform;

        _lassoRenderer = new LassoRenderer();
        _lassoRenderer.lineRenderer = GetComponent<LineRenderer>();
        _lassoRenderer.lassoRopeSegments = numberOfLassoRopeSegments;
        _lassoRenderer.lassoLoopSegments = numberOfLassoLoopSegments;
        _lassoRenderer.wiggleSpeed = lassoWiggleSpeed;
        _lassoRenderer.amplitude = lassoWiggleAmplitude;
        _lassoRenderer.lassoRadius = lassoRadius;
        _lassoRenderer.swingLayerMask = lassoLayerMask;
        _lassoRenderer.playerTransform = transform;
        _lassoRenderer.StopRendering();
    }
    void Update()
    {
        // Later, we will need to ignore input to the player controller
        // when in the UI or when in cutscenes
        if (true)
        {
            if (_state == PlayerState.SWING)
            {
                GetSwingInput();
            } 
            else if (_state == PlayerState.THROWING_LASSO)
            {
                // Allow for cancel throw if the player inputs anything
                print("Lasso target to throw at is: " + _lassoHitTransform.position);
            }
            else // if (_state != PlayerState.THROWING_LASSO)
            {
                GetMoveInput();
                AimLasso();
                GetLassoInput();
            }
        }
    }

    void FixedUpdate()
    {
        if (_state == PlayerState.SWING)
        {
            Swing();
        } 
        else
        {
            if (!_gravityObject.IsInSpace())
            {
                Run();
            }
        }
    }
    private void LateUpdate()
    {
        DrawLasso();
    }

    void UpdateState(PlayerState newState)
    {
        if (_state != newState)
        {
            // NOTE: moved this line into if statement - otherwise, never !=
            _state = newState;
            // Signal update to animation
            UpdateAnimState();
            if (_state == PlayerState.AIR)
            {
                // Whenever we enter the AIR state, we should detect the next time we land on the ground.
                _detectLanding = true;
            }
        }
        _state = newState;
    }

    void UpdateAnimState()
    {
        if (playerAnimator == null) { return; }
        // This handles the changes to the animimations based on the player state
        if (_state == PlayerState.IDLE)
        {
            playerAnimator.Play("Base Layer.BC_Idle");
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.WALK)
        {
            playerAnimator.Play("Base Layer.BC_Walk");
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.RUN)
        {
            playerAnimator.Play("Base Layer.BC_Walk");
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.SWING)
        {
            playerAnimator.SetLayerWeight(1, 1.0f);
        }
        if (_state == PlayerState.AIR)
        {
            playerAnimator.Play("Base Layer.BC_Walk");
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
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
        if (Input.GetKeyDown(lassoKey)) {
            _hitLassoTarget = HitLassoTarget.NONE;
            if (_lassoRaycastHit.point != Vector3.zero)
            {
                // Move lassoHit to the location of the lasso hitpoint
                _lassoHitTransform.position = _lassoRaycastHit.point;
                _hitObjectTransform = _lassoRaycastHit.collider.gameObject.transform;
                if (_lassoRaycastHit.collider.gameObject.GetComponent<SwingableObject>() != null)
                {
                    _hitLassoTarget = HitLassoTarget.SWINGABLE;
                }
                ThrowLasso();
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

        float radius = lassoAimAssistMinRadius;
        float delta = (lassoAimAssistMaxRadius - radius) / ((float) lassoAimAssistNumIter - 1.0f);

        Vector3 aimDirection = _cameraTransform.forward;
        RaycastHit sphereCastHit;
        // This 'if' statement is here b/c C# doesn't like unassigned variables
        // so sphereCastHit needs a garaunteed value :/
        if (!Physics.SphereCast(_cameraTransform.position, radius, aimDirection,
            out sphereCastHit, lassoRange, lassoLayerMask, QueryTriggerInteraction.Ignore))
        {
            for (int i = 1; i < lassoAimAssistNumIter; i++)
            {
                float curRadius = radius + delta * i;
                if (Physics.SphereCast(_cameraTransform.position, curRadius, aimDirection, 
                    out sphereCastHit, lassoRange, lassoLayerMask, QueryTriggerInteraction.Ignore))
                {
                    break;
                }

            }
        }

        RaycastHit raycastHit;
        Physics.Raycast(_cameraTransform.position, aimDirection, 
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

    void ThrowLasso()
    {
        UpdateState(PlayerState.THROWING_LASSO);

        _lassoRenderer.Throw(lassoThrowPosition, _lassoHitTransform, lassoTimeToHit);
        if (_hitLassoTarget == HitLassoTarget.SWINGABLE)
        {
            Invoke("StartSwing", lassoTimeToHit);
        } 
        else
        {
            Invoke("HitNothing", lassoTimeToHit);
        }
        _cancelLassoThrow = false;
    }

    void HitNothing()
    {
        UpdateState(PlayerState.IDLE);
        _lassoRenderer.StopRendering();
    }

    void ThrowLassoInput()
    {
        // Here we detect input if the player wants to cancel a throw while it is happening.
        // This may require fiddling with numbers to get it feeling right.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        _moveInput = new Vector3(horizontal, 0, vertical).normalized; // Update move input so it carries over to the next input

        // I think WASD canceling might feel wrong, so maybe cancel on jump?
        if (Input.GetKeyDown(jumpKey)) { 
            _cancelLassoThrow = true;
            UpdateState(PlayerState.IDLE); // Will be corrected next update, just get the player out of the throwing state
        }
    }

    void DrawLasso()
    {
        if (_state == PlayerState.THROWING_LASSO)
        {
            _lassoRenderer.RenderThrow();
        } 
        else if (_state == PlayerState.SWING)
        {
            _lassoRenderer.RenderSwing(_hitObjectTransform);
        }
    }

    void StartSwing()
    {
        if (_cancelLassoThrow) { return; }

        Vector3 dirToPlayer = (transform.position - _lassoRaycastHit.point).normalized;
        Vector3 axisOfSwing = Vector3.Cross(_cameraTransform.transform.forward, dirToPlayer);
        if (axisOfSwing.magnitude == 0)
        {
            return;
        }
        UpdateState(PlayerState.SWING);

        _rigidBody.isKinematic = true; // Allows us to directly control the player's position so we can move them in a perfect arc
        _gravityObject.disabled = true;

        _swingRadius = Mathf.Clamp(Vector3.Distance(transform.position, _lassoHitTransform.position), minSwingRadius, maxSwingRadius);
        _lassoHitTransform.rotation = Quaternion.FromToRotation(_lassoHitTransform.right, dirToPlayer) * _lassoHitTransform.rotation;
        _lassoHitTransform.rotation = Quaternion.FromToRotation(_lassoHitTransform.up, axisOfSwing) * _lassoHitTransform.rotation;


        _swingProgress = 0.0f;
        _swingVelocity = startingSwingSpeed;
    }

    void Swing()
    {
        // Move along a parametric curve
        // For now, lock the player right where they need to be.
        Vector3 prev = transform.position;
        transform.position = _lassoHitTransform.position
                + _swingRadius * Mathf.Cos(_swingProgress) * _lassoHitTransform.right
                + _swingRadius * Mathf.Sin(_swingProgress) * _lassoHitTransform.forward;
        _swingProgress += _swingVelocity * Time.deltaTime;

        if (_swingProgress > distanceOfSwing)
        {
            EndSwing();
        }
        
        // Move input
        _swingVelocity = Mathf.Clamp(_swingVelocity + swingAccelRate * _moveInput.z, startingSwingSpeed, maxSwingSpeed);
        if (_moveInput.x > 0)
        {
            _lassoHitTransform.Rotate(new Vector3(-swingSideRotateSpeed, 0, 0), Space.Self);
        } 
        else if (_moveInput.x < 0)
        {
            _lassoHitTransform.Rotate(new Vector3(swingSideRotateSpeed, 0, 0), Space.Self);
        }

        // Re-orienting model
        Vector3 dirToPoint = (_lassoHitTransform.position - transform.position).normalized;
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

        _lassoRenderer.StopRendering();

        _rigidBody.AddForce(endSwingVerticalBoostForce * transform.up + model.transform.forward * endSwingBoostForce * (_swingVelocity / maxSwingSpeed), ForceMode.Impulse);
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
    }
}
public class LassoRenderer
{
    public Transform playerTransform;
    public LineRenderer lineRenderer;
    public int lassoLoopSegments;
    public int lassoRopeSegments;
    public float amplitude;
    public float wiggleSpeed;
    public float lassoRadius;
    public LayerMask swingLayerMask;

    private Transform _start;
    private Transform _target;
    private float _timeToHitTarget;
    private float _accumTime;
    private Vector3 _up;
    private Vector3 _forward; // Tthe direction the rope is being thrown
    private Vector3 _right;

    public void Throw(Transform start, Transform target, float timeToHitTarget)
    {
        _start = start;
        _target = target;
        _timeToHitTarget = timeToHitTarget;
        lineRenderer.positionCount = lassoLoopSegments + lassoRopeSegments;
        _accumTime = 0;
    }

    public void RenderThrow()
    {
        if (lineRenderer.positionCount == 0) { return; }

        // Calculations for current rope length and basis for rope
        _accumTime += Time.deltaTime;
        float currentLength = (_accumTime / _timeToHitTarget) * Vector3.Distance(_start.position, _target.position);
        Vector3 _forward = (_target.position - _start.position).normalized;
        Vector3 _up = Vector3.Cross(_forward, _start.forward).normalized;
        Vector3 _right = Vector3.Cross(_forward, _up).normalized;

        // Rendering the lasso rope
        for (int i = 0; i < lassoRopeSegments; i++)
        {
            float percentAlongLine = ((float)i / (float)lassoRopeSegments);
            Vector3 pos = percentAlongLine * currentLength * _forward + _start.position;
            if (i != 0)
            {
                pos += Mathf.Sin(_accumTime * wiggleSpeed * percentAlongLine * 0.5f) * amplitude * percentAlongLine * _up;
            }
            lineRenderer.SetPosition(i, pos);
        }

        // Rendering lasso loop
        Vector3 lassoCenterPos = _start.position + (currentLength + lassoRadius) * _forward;

        for (int i = 0; i < lassoLoopSegments; i++)
        {
            float theta = ((float) i /  (float) lassoLoopSegments) * Mathf.PI * 2;
            Vector3 pos = lassoCenterPos
                + Mathf.Cos(theta) * -lassoRadius * (_accumTime / _timeToHitTarget) * _forward
                + Mathf.Sin(theta) * lassoRadius * (_accumTime / _timeToHitTarget) * _right
                + Mathf.Cos(_accumTime * wiggleSpeed) * 0.2f * _right
                + Mathf.Sin(_accumTime * wiggleSpeed) * 0.2f * _forward
                + Mathf.Cos((_accumTime / _timeToHitTarget) * wiggleSpeed * theta / 10) * 0.5f * _up;

            if (i + 1 == lassoLoopSegments || i == 0)
            {
                // To ensure a loop is complete
                pos = lassoCenterPos 
                    + Mathf.Cos(0) * -lassoRadius * _forward 
                    + Mathf.Sin(0) * lassoRadius * _right;
            }
            lineRenderer.SetPosition(i + lassoRopeSegments, pos);
        }
    }

    public void RenderSwing(Transform swingObjectTransform)
    {
        if (lineRenderer.positionCount == 0) { return; }

        float length = Vector3.Distance(_start.position, _target.position);
        Vector3 dir = (_target.position - _start.position).normalized;

        // Render the lasso rope
        for (int i = 0; i < lassoRopeSegments; i++)
        {
            float percentageAlongRope = ((float)i / (float)lassoRopeSegments);
            Vector3 pos = percentageAlongRope * length * dir + _start.position;
            lineRenderer.SetPosition(i, pos);
        }

        Vector3 center = swingObjectTransform.position;
        Vector3 forward = (_target.position - center).normalized;
        Vector3 up = Vector3.Cross(Vector3.Cross(forward, playerTransform.up).normalized, forward).normalized;
        float dist = (_target.position - center).magnitude * 0.2f;

        for (int i = 0; i < lassoLoopSegments; i++)
        {
            float theta = ((float)i / (float)lassoLoopSegments) * Mathf.PI * 2;
            Vector3 exteriorPos = center
                    + Mathf.Cos(theta) * 5f * forward
                    + Mathf.Sin(theta) * 5f * up;

            Vector3 toCenter = (center - exteriorPos).normalized;
            Vector3 pos = exteriorPos;
            RaycastHit hit;

            if (i == 0 || i + 1 == lassoLoopSegments)
            {
                pos = _target.position 
                    + Mathf.Cos(0) * dist * forward
                    + Mathf.Sin(0) * dist * up;
            } 
            else if (Physics.Raycast(exteriorPos, toCenter, out hit, 20.0f, swingLayerMask, QueryTriggerInteraction.Ignore))
            {
                pos = hit.point
                    + Mathf.Cos(theta) * dist * forward
                    + Mathf.Sin(theta) * dist * up;
            }

            lineRenderer.SetPosition(i + lassoRopeSegments, pos);
        }
    }

    public void StopRendering()
    {
        lineRenderer.positionCount = 0;
    }
}
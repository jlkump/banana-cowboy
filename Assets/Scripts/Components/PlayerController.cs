using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(GravityObject))]
public class PlayerController : MonoBehaviour
{
    [Header("Debug")]


    [Header("References")]
    public Transform model;
    public Transform emptyObjectPrefab;
    public Animator playerAnimator = null;
    public UIManager playerUI;
    // We can find these ourselves
    Rigidbody _rigidBody;
    Transform _cameraTransform;
    GravityObject _gravityObject;
    public Joystick joystick;
    public GameObject mobileControls;
    private Camera _camera;


    [Header("Movement")]
    [Tooltip("The maximum walk speed.")]
    public float walkSpeed = 8.0f;
    [Tooltip("The maximum run speed.")]
    public float runSpeed = 12.0f;

    float horizontal = 0;
    float vertical = 0;

    [Tooltip("The rate of speed increase for getting to max walk / run speed")]
    public float accelerationRate = (50f * 0.5f) / 8.0f;  // The rate of speed increase
    [Tooltip("The rate of speed decrease when slowing the player down to no movement input")]
    public float deccelerationRate = (50f * 0.5f) / 8.0f; // The rate of speed decrease (when no input is pressed)
    [Range(0.0f, 1.0f),Tooltip("The ratio for the player to control the player while in the air relative to ground movement. Scale [0, 1.0]. 0.5 means 50% the effective acceleration in the air relative to the ground.")]
    public float accelAirControlRatio = 0.8f;       // The ability for the player to re-orient themselves in the air while accelerating
    [Range(0.0f, 1.0f),Tooltip("The ratio for the player to control the player while in the air relative to ground movement, same as Accel Air Control Ratio, but how much is lost when the player is slowing down in the air.")]
    public float deccelAirControlRatio = 0.8f;      // The ability for the player to move themselves while in the air and deccelerating (range [0.0,1.0])
    public float jumpImpulseForce = 10.0f;
    [Tooltip("Determines how much the force of gravity is increased when the jump key is released.")]
    public float gravIncreaseOnJumpRelease = 3.0f;
    [Tooltip("If the player somehow achieves speed greater than the maximum allowed, we won't give them any more speed. However, with conserve momentum, we won't reduce their speed either.")]
    public bool conserveMomentum = true;
    private float _jumpBufferTimer;
    private float _jumpBuffer = 0.2f;

    public float walkAnimSpeed = 1.0f;
    public float runAnimSpeed = 1.0f;

    private Vector3 _moveInput;
    private float _lastTimeOnGround = 0.0f;
    private bool _isRunning = false;
    
    // This tracks the velocity of the player's lateral movement at the time of jumping so we restore it when landing
    private Vector3 _jumpMoveVelocity; 
    private bool _detectLanding = false;

    [Header("Lasso Throw")]
    public Transform lassoThrowPosition;
    [Tooltip("The distance that the lasso can aim to. The player will be pulled to the max distance the lasso can reach for the action.")]
    public float lassoTimeToHit = 0.3f;
    [SerializeField, Range(2, 100), Tooltip("The number of sphere-casts we perform per-frame for aim-assist")]
    int _aimAssistRaycastResolution = 10;

    [SerializeField, Range(0.01f, 50f)]
    float _aimAssistMinRadius = 0.4f, _aimAssistMaxRadius = 10f, _cameraRaycastRadius = 0.1f, lassoAimRange = 40.0f;
    [SerializeField, Range(0.1f, 50.0f), Tooltip("The distance between the camera and object in which the object is ignored.")]
    float _lassoIgnoreDist = 10f;

    public LayerMask lassoLayerMask;

    enum HitLassoTarget
    {
        NONE,
        SWINGABLE,
        ENEMY,
        OBJECT
    }
    private Transform _lassoHitPointTransform; // This is NOT the transform of the object hit, but the transform of the specific raycast hit
    private Transform _lassoHitObjectTransform; // The transform of the object actually hit
    private Rigidbody _lassoHitObjectRigidBody = null;
    private RaycastHit _lassoRaycastHit;
    private LassoObject _lassoSelectedObject = null;
    private HitLassoTarget _hitLassoTarget;
    private bool _cancelLassoAction;
    private bool _canThrowLasso = true;
    private float _lassoThrowCooldown; // Is the length of the throw SFX (at minimum)



    [Header("Lasso Rendering")]
    public int numberOfLassoLoopSegments = 15;
    public int numberOfLassoRopeSegments = 15;
    public float lassoRadius = 0.3f;
    public float lassoWiggleSpeed = 20.0f;
    public float lassoWiggleAmplitude = 1.5f;
    public float lassoWiggle = 50.0f;

    private LassoRenderer _lassoRenderer;

    [Header("Lasso Swinging")]
    [SerializeField]
    float swingRadius;
    [SerializeField]
    CharacterJoint _swingJointPrefab;
    CharacterJoint _activeSwingJoint;


    [Header("Lasso Holding")]
    public float holdHeight = 4.0f;
    public float tossForwardImpulse = 30.0f;
    public float tossUpwardImpulse = 10.0f;
    public float timeToPullObject = 0.3f;
    public float timeToToss = 0.1f;
    public float percentageFarThrow = 0.2f;
    public float percentageMidThow = 0.3f;
    [Tooltip("The speed of the indicator for the throwing mini-game.")]
    public float lassoIndicatorSpeed = 1.6f;

    public float holdSwingRadius = 4.0f;
    public float holdSwingSpeed = 1.0f;

    private float _accumPullTime;
    private float _accumHoldTime;

    public enum ThrowStrength
    {
        WEAK,
        MEDIUM,
        STRONG
    }


    [Header("Input")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode lassoKey = KeyCode.Mouse0;

    [Header("Health")]
    public int health;
    public static int maxHealth = 3;
    private bool _canTakeDamage; // Invincibilty frames
    public GameObject characterRender;
    public Material normalColor;
    public Material damageColor;

    enum PlayerState
    {
        IDLE,
        AIR,
        WALK,
        RUN,
        THROW_LASSO,
        GRAPPLE,
        SWING,
        PULL,
        HOLD,
        TOSS,
    };
    

    private PlayerState _state = PlayerState.AIR;

    private bool _disableForCutscene = false;

    // use coroutine to set player position
    IEnumerator SetSpawnPos()
    {
        yield return new WaitForEndOfFrame();
        if (GameObject.Find("CheckpointManager") != null)
        {
            print("Normal Levels");
            transform.position = LevelData.getRespawnPos();
        }
        else
        {
            print("No checkpoints needed");
            transform.position = Vector3.zero;
        }
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _gravityObject = GetComponent<GravityObject>();
#if UNITY_IOS || UNITY_ANDROID
        mobileControls.SetActive(true);
#else
        mobileControls.SetActive(false);
#endif
        // TODO: TEST IF PUT HERE OR IN START
        StartCoroutine(SetSpawnPos());

        if (playerAnimator != null)
        {
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
#if UNITY_IOS
        GameObject.Find("Mobile Manager").SetActive(true);
        mobileControls.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
#endif
    }

    void Start()
    {
        _camera = Camera.main;
        _cameraTransform = _camera.transform;
        _lassoHitPointTransform = Instantiate(emptyObjectPrefab, transform.position, Quaternion.identity).transform;

        _lassoRenderer = new LassoRenderer();
        _lassoRenderer.lineRenderer = GetComponent<LineRenderer>();
        _lassoRenderer.lassoRopeSegments = numberOfLassoRopeSegments;
        _lassoRenderer.lassoLoopSegments = numberOfLassoLoopSegments;
        _lassoRenderer.wiggleSpeed = lassoWiggleSpeed;
        _lassoRenderer.amplitude = lassoWiggleAmplitude;
        _lassoRenderer.wiggleness = lassoWiggle;
        _lassoRenderer.lassoRadius = lassoRadius;
        _lassoRenderer.swingLayerMask = lassoLayerMask;
        _lassoRenderer.playerTransform = transform;
        _lassoRenderer.StopRendering();

        //_lassoThrowCooldown = SoundManager.S_Instance().GetSound("LassoThrow").src.clip.length;
        _lassoThrowCooldown = 0.5f;

/*        StartCoroutine(SetSpawnPos());
*/
        health = maxHealth;
        _canTakeDamage = true;
        playerUI.SetAbsHealth(health);
    }
    void Update()
    {
        // Later, we will need to ignore input to the player controller
        // when in the UI or when in cutscenes
        
        if (!PauseManager.pauseActive && !_disableForCutscene)
        {
            switch(_state)
            {
                case PlayerState.THROW_LASSO:
                    GetThrowLassoInput();
                    break;
                case PlayerState.GRAPPLE:
                    GetGrappleInput();
                    break;
                case PlayerState.SWING:
                    GetSwingInput();
                    break;
                case PlayerState.PULL:
                    GetPullInput();
                    break;
                case PlayerState.HOLD:
                    GetHoldInput();
                    UpdateMoveHoldAnim();
                    break;
                case PlayerState.TOSS:
                    GetTossInput();
                    break;
                default:
                    GetMoveInput();
                    AimLasso();
                    GetLassoInput();
                    break;
            }
        }
    }

    void FixedUpdate()
    {
        switch(_state)
        {
            case PlayerState.GRAPPLE:
                Grapple(); 
                break;
            case PlayerState.SWING:
                Swing();
                break;
            case PlayerState.PULL:
                Pull();
                break;
            case PlayerState.HOLD:
                Hold();
                Run();
                break;
            default:
                if (!_gravityObject.IsInSpace())
                {
                    Run();
                }
                break;
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
            SoundManager.Instance().StopSFX("PlayerRun");
            SoundManager.Instance().StopSFX("PlayerWalk");

            // Signal update to animation
            UpdateAnimState();
            switch (_state)
            {
                case PlayerState.AIR:
                    // Whenever we enter the AIR state, we should detect the next time we land on the ground.
                    _detectLanding = true;
                    break;
                case PlayerState.WALK:
                    SoundManager.Instance().PlaySFX("PlayerWalk");
                    break;
                case PlayerState.RUN:
                    SoundManager.Instance().PlaySFX("PlayerRun");
                    break;
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
            playerAnimator.speed = 1.0f;
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.WALK)
        {
            playerAnimator.Play("Base Layer.BC_Walk");
            playerAnimator.speed = walkAnimSpeed;
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.RUN)
        {
            playerAnimator.Play("Base Layer.BC_Run");
            playerAnimator.speed = runAnimSpeed;
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.SWING)
        {
            playerAnimator.Play("Base Layer.BC_Swing");
            // playerAnimator.SetLayerWeight(1, 1.0f);
        }
        if (_state == PlayerState.AIR)
        {
            playerAnimator.Play("Base Layer.BC_Fall");
            playerAnimator.speed = 1.5f;
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        if (_state == PlayerState.THROW_LASSO)
        {
            playerAnimator.Play("Base Layer.BC_Lasso");
            playerAnimator.SetLayerWeight(1, 0.0f);
            playerAnimator.speed = 1.5f;
        }
        if (_state == PlayerState.HOLD)
        {
            playerAnimator.Play("Lasso Layer.BC_Hold");
            playerAnimator.SetLayerWeight(1, 1.0f);
            playerAnimator.speed = 1.5f;
        }
        if (_state == PlayerState.TOSS)
        {
            playerAnimator.Play("Base Layer.BC_Lasso");
            playerAnimator.SetLayerWeight(1, 0.0f);
            playerAnimator.speed = 1.5f;
            print("TOSSSSSS");
        }
        
    }

    void UpdateMoveHoldAnim()
    {
        if (_moveInput.magnitude > 0)
        {
            if (_isRunning)
            {
                playerAnimator.Play("Base Layer.BC_Run");
                playerAnimator.speed = runAnimSpeed;
                playerAnimator.SetLayerWeight(1, 0.0f);
            }
            else
            {
                playerAnimator.Play("Base Layer.BC_Walk");
                playerAnimator.speed = walkAnimSpeed;
                playerAnimator.SetLayerWeight(1, 0.0f);
            }
        }
        else
        {
            playerAnimator.Play("Base Layer.BC_Idle");
            playerAnimator.speed = 1.0f;
            playerAnimator.SetLayerWeight(1, 0.0f);
        }
        playerAnimator.SetLayerWeight(1, 1.0f);
    }

    bool IsValidState(PlayerState newState)
    {
        return true;
    }

    void GetMoveInput()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
#else

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
#endif

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
        _jumpBufferTimer -= Time.deltaTime;
        if (_gravityObject.IsOnGround())
        {
            if (_detectLanding)
            {
                // This is how we detect if the player has landed or not.
                // It is called once, only on landing after falling off a ledge or jumping.
                OnLand();
                //if (_jumpBufferTimer > 0)
                //{
                //    print("Jump Buffered: "+_jumpBufferTimer);
                //    StartJump();
                //    _jumpBufferTimer = 0;
                //}
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
#if !UNITY_IOS && !UNITY_ANDROID
        if (Input.GetKeyDown(jumpKey)) 
        {
            // Last time on ground acts as a coyote timer for jumping
            _jumpBufferTimer = _jumpBuffer;

            if (_lastTimeOnGround > 0)
            {
                StartJump();
            }
/*            else if (false)
            {
                // this is here for jump buffering
            }*/

        } 
/*        else if (false)
        {
            // Again, this is here for jump buffering
        }*/
        else if (Input.GetKeyUp(jumpKey))
        {
            EndJump();
        }
#endif
    }

    public void PressJumpMobile()
    {
        PlayerCameraController.uiTouching++;
        _jumpBufferTimer = _jumpBuffer;
        if (_lastTimeOnGround > 0)
        {
            StartJump();
        }
    }
    public void ReleaseJumpMobile()
    {
        PlayerCameraController.uiTouching--;
        EndJump();
    }

    public void PressLassoMobile()
    {
        PlayerCameraController.uiTouching++;
        LassoTargeting();
    }

    public void ReleaseLassoMobile()
    {
        PlayerCameraController.uiTouching--;
        switch (_state)
        {
            case PlayerState.THROW_LASSO:
                CancelLasso();
                break;

            case PlayerState.PULL:
                _cancelLassoAction = true;
                _lassoSelectedObject.currentlyLassoed = false;
                _lassoHitObjectRigidBody.isKinematic = false;
                _lassoHitObjectRigidBody.AddForce((transform.position - _lassoHitPointTransform.position).normalized * tossForwardImpulse * 0.5f,
                    ForceMode.Impulse);
                UpdateState(PlayerState.IDLE);
                break;

            case PlayerState.GRAPPLE:
                EndGrapple();
                break;
            case PlayerState.HOLD:
                StartToss();
                break;

            case PlayerState.SWING:
                _lassoSelectedObject.currentlyLassoed = false;
                EndSwing();
                break;
        }
    }

    public void PauseMobile()
    {
        PauseManager temp = GameObject.Find("Pause Manager").GetComponent<PauseManager>();
        temp.PauseGame();
    }

    public void RunMobile()
    {
        _isRunning = _isRunning ? false: true;
        if (_isRunning)
        {
            GameObject.Find("Run").GetComponent<UnityEngine.UI.Image>().color = Color.red;
        }
        else
        {
            GameObject.Find("Run").GetComponent<UnityEngine.UI.Image>().color = Color.white;
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
        _moveInput = 
            Vector3.Dot(_gravityObject.gravityOrientation.right, _moveInput) * _gravityObject.gravityOrientation.right 
            + Vector3.Dot(_gravityObject.gravityOrientation.forward, _moveInput) * _gravityObject.gravityOrientation.forward;
        Vector3 targetVelocity = _moveInput.normalized * (_state == PlayerState.RUN ? runSpeed : walkSpeed);

        float accelRate;
        // Gets an acceleration value based on if we are accelerating (includes turning) 
        // or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (_lastTimeOnGround > 0)
            accelRate = (Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0) ? accelerationRate : deccelerationRate;
        else
            accelRate = (Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0) ? accelerationRate * accelAirControlRatio : deccelerationRate * deccelAirControlRatio;


        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        if (conserveMomentum &&
            _gravityObject.GetMoveVelocity().magnitude > targetVelocity.magnitude &&
            Vector3.Dot(targetVelocity, _gravityObject.GetMoveVelocity()) > 0 &&
            targetVelocity.magnitude > 0.01f && _lastTimeOnGround < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }

        Vector3 speedDiff = targetVelocity - _gravityObject.GetMoveVelocity();
        Vector3 movement = speedDiff * accelRate;
        if (_moveInput.magnitude > 0)
        {
            _rigidBody.AddForce(movement);
            Debug.DrawRay(transform.position, targetVelocity, Color.blue);
            Debug.DrawRay(transform.position, movement, Color.magenta);
        } 
        else
        {
            _rigidBody.velocity = Vector3.zero + _gravityObject.GetFallingVelocity();
        }

        // Spin player model and orientation to right direction to face
        if (_moveInput.magnitude > 0 && model != null)
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(targetVelocity.normalized, model.transform.up), Time.deltaTime * 8);
        }
    }

    void AimLasso()
    {
        if (!_canThrowLasso) { return; }
        // Do a simple raycast from the camera out forward where the camera is looking
        RaycastHit hit;
        if (Physics.SphereCast(_cameraTransform.position, _cameraRaycastRadius, _cameraTransform.forward,
            out hit, lassoAimRange + Vector3.Distance(_cameraTransform.position, transform.position), 
            lassoLayerMask, QueryTriggerInteraction.Ignore) &&
            hit.collider != null && hit.collider.gameObject != null &&
            hit.collider.gameObject.GetComponentInParent<LassoObject>() != null &&
            hit.collider.gameObject.GetComponentInParent<LassoObject>().isLassoable)
        {

            // Looking directly at an object
            // Nothing to do here for now
        }
        else
        {
            hit.point = Vector3.zero;
        }

        // We are not directly looking at anything, so use aim-assist
        if (hit.point == Vector3.zero)
        {
            float deltaRadius = (_aimAssistMaxRadius - _aimAssistMinRadius) / (_aimAssistRaycastResolution - 1);
            float closestDist = float.MaxValue;

            Vector3 centerCameraPoint = new Vector3(0.5f, 0.5f, 0.0f);

            for (int i = 0; i < _aimAssistRaycastResolution; i++)
            {
                float raycastRadius = _aimAssistMinRadius + deltaRadius * i;
                RaycastHit raycastHit;
                if (Physics.SphereCast(_cameraTransform.position, raycastRadius, _cameraTransform.forward,
                    out raycastHit, lassoAimRange + Vector3.Distance(_cameraTransform.position, transform.position), 
                    lassoLayerMask, QueryTriggerInteraction.Ignore) &&
                    raycastHit.collider != null && raycastHit.collider.gameObject != null &&
                    raycastHit.collider.gameObject.GetComponentInParent<LassoObject>() != null &&
                    raycastHit.collider.gameObject.GetComponentInParent<LassoObject>().isLassoable &&
                    _camera.WorldToViewportPoint(raycastHit.point).z > _lassoIgnoreDist)
                {
                    Vector3 cameraSpacePoint = _camera.WorldToViewportPoint(raycastHit.point);
                    cameraSpacePoint.z = 0;
                    if (Vector3.Distance(cameraSpacePoint, centerCameraPoint) < closestDist)
                    {
                        closestDist = Vector3.Distance(cameraSpacePoint, centerCameraPoint);
                        hit = raycastHit;
                    }
                }
            }
        }

        // We found an actual hit
        if (hit.point != Vector3.zero)
        {
            if (_lassoSelectedObject != null && hit.collider.gameObject.GetComponentInParent<LassoObject>() != _lassoSelectedObject)
            {
                _lassoSelectedObject.Deselect();
            }
            _lassoSelectedObject = hit.collider.gameObject.GetComponentInParent<LassoObject>();
            _lassoSelectedObject.Select();
            _lassoRaycastHit = hit;
            playerUI.ReticleOverLassoable();

        }
        else
        {
            _lassoRaycastHit.point = Vector3.zero;
            if (_lassoSelectedObject != null)
            {
                _lassoSelectedObject.Deselect();
                _lassoSelectedObject = null;
                _lassoHitObjectRigidBody = null;
                _lassoHitObjectTransform = null;
            }
            playerUI.ReticleReset();
        }
    }

    void GetLassoInput()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        if (Input.GetKeyDown(lassoKey))
        {
            LassoTargeting();
        }
#endif
    }

    void LassoTargeting()
    {
        _hitLassoTarget = HitLassoTarget.NONE;
        if (_lassoRaycastHit.point != Vector3.zero && _lassoSelectedObject != null)
        {
            // Move lassoHit to the location of the lasso hitpoint
            _lassoHitPointTransform.position = _lassoRaycastHit.point;
            _lassoHitObjectTransform = _lassoRaycastHit.collider.gameObject.transform;
            _lassoHitObjectRigidBody = _lassoRaycastHit.collider.gameObject.GetComponentInParent<Rigidbody>();

            if (_lassoSelectedObject is SwingableObject)
            {
                _hitLassoTarget = HitLassoTarget.SWINGABLE;
            }
            else if (_lassoSelectedObject is LassoableEnemy)
            {
                _hitLassoTarget = HitLassoTarget.ENEMY;
            }
            ThrowLasso();
        }
    }

    void ThrowLasso()
    {
        if (_canThrowLasso)
        {
            _canThrowLasso = false;
            _cancelLassoAction = false;

            UpdateState(PlayerState.THROW_LASSO);

            SoundManager.Instance().PlaySFX("LassoThrow");

            _lassoRenderer.Throw(lassoThrowPosition, _lassoHitPointTransform, lassoTimeToHit);

            _lassoSelectedObject.currentlyLassoed = true;

            if (_hitLassoTarget == HitLassoTarget.SWINGABLE)
            {
                Invoke("StartSwing", lassoTimeToHit);
            } 
            else if (_hitLassoTarget == HitLassoTarget.ENEMY)
            {
                Invoke("StartPull", lassoTimeToHit);
            }
            else
            {
                Invoke("HitNothing", lassoTimeToHit);
            }

            Invoke("LassoCooldown", _lassoThrowCooldown);
        }
    }

    void LassoCooldown()
    {
        _canThrowLasso = true;
    }

    void GetThrowLassoInput()
    {
        // Here we detect input if the player wants to cancel a throw while it is happening.
        // This may require fiddling with numbers to get it feeling right.
#if UNITY_IOS || UNITY_ANDROID
        if (joystick != null)
        {
           horizontal = joystick.Horizontal;
           vertical = joystick.Vertical;
        }
#else

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
#endif
        _moveInput = new Vector3(horizontal, 0, vertical).normalized; // Update move input so it carries over to the next input

#if !UNITY_IOS && !UNITY_ANDROID
        // Cancel on lift lmb
        if (Input.GetKeyUp(lassoKey)) {
            CancelLasso();
        }
#endif
    }

    void CancelLasso()
    {
        _cancelLassoAction = true;
        _lassoSelectedObject.currentlyLassoed = false;
        UpdateState(PlayerState.IDLE); // Will be corrected next update, just get the player out of the throwing state
    }

    void HitNothing()
    {
        SoundManager.Instance().PlaySFX("LassoMiss");
        UpdateState(PlayerState.IDLE);
    }

    void DrawLasso()
    {
        switch(_state)
        {
            case PlayerState.GRAPPLE:
                _lassoRenderer.RenderSwing(_lassoHitObjectTransform); // Might make a RenderGrapple later
                break;
            case PlayerState.THROW_LASSO:
                _lassoRenderer.RenderThrow();
                break;
            case PlayerState.SWING:
                _lassoRenderer.RenderSwing(_lassoHitObjectTransform);
                break;
            case PlayerState.PULL:
                _lassoRenderer.RenderPullin(_lassoHitObjectTransform);
                break;
            case PlayerState.HOLD:
                _lassoRenderer.RenderHoldObject(_lassoHitObjectTransform);
                break;
            case PlayerState.TOSS:
                _lassoRenderer.RenderTossEnemy();
                break;
            default:
                _lassoRenderer.StopRendering();
                break;
        }
    }

    void StartGrapple()
    {

    }

    void Grapple()
    {

    }

    void EndGrapple()
    {

    }

    void GetGrappleInput()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        if (Input.GetKeyUp(lassoKey))
        {
            EndGrapple();
        }
#endif
    }

    void StartSwing()
    {
        UpdateState(PlayerState.SWING);
        _activeSwingJoint = Instantiate(_swingJointPrefab);
        _activeSwingJoint.autoConfigureConnectedAnchor = false;
        _activeSwingJoint.anchor = (_lassoHitObjectTransform.position - transform.position).normalized * swingRadius;
        _activeSwingJoint.connectedAnchor = _lassoHitObjectTransform.position;
        _activeSwingJoint.transform.position = (transform.position - _lassoHitObjectTransform.position).normalized * swingRadius + _lassoHitObjectTransform.position;

        Vector3 prevVel = _rigidBody.velocity;

        _rigidBody.isKinematic = true;
        transform.position = _activeSwingJoint.transform.position;
        _rigidBody.isKinematic = false;

        FixedJoint playerJoint = gameObject.AddComponent<FixedJoint>();
        playerJoint.connectedBody = _activeSwingJoint.GetComponent<Rigidbody>();

        _gravityObject.gravityMult = 3f;
        //_rigidBody.AddForce(prevVel * 10f, ForceMode.Impulse);
    }

    void Swing()
    {
        _moveInput = _cameraTransform.TransformDirection(_moveInput);
        // Transform the move input relative to the player
        _moveInput =
            Vector3.Dot(_gravityObject.gravityOrientation.right, _moveInput) * _gravityObject.gravityOrientation.right
            + Vector3.Dot(_gravityObject.gravityOrientation.forward, _moveInput) * _gravityObject.gravityOrientation.forward;
        Vector3 targetVelocity = _moveInput * 20f;
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
        if (_gravityObject.IsInSpace())
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.FromToRotation(model.up, _cameraTransform.up) * model.rotation, Time.deltaTime * 8);
        } 
        else
        {
            if (_moveInput.magnitude > 0 && model != null)
            {
                model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(targetVelocity.normalized, _gravityObject.gravityOrientation.up), Time.deltaTime * 8);
            }
        }
    }

    void EndSwing()
    {
        UpdateState(PlayerState.AIR);

        _lassoRenderer.StopRendering();

        Vector3 prevVel = _rigidBody.velocity;

        Destroy(GetComponent<FixedJoint>());
        Destroy(_activeSwingJoint.gameObject);

        _gravityObject.gravityMult = 1f;

        _rigidBody.AddForce(prevVel, ForceMode.Impulse);

        Invoke("SwingBoost", 0.1f);

        SoundManager.Instance().StopSFX("LassoSwing");
    }

    void SwingBoost()
    {
    }

    void GetSwingInput()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
#else

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
#endif
        _moveInput = new Vector3(horizontal, 0, vertical).normalized; // Re-using _moveInput, cause why not

#if !UNITY_IOS && !UNITY_ANDROID
        if (Input.GetKeyUp(lassoKey)) {
            _lassoSelectedObject.currentlyLassoed = false;
            EndSwing(); 
        }
#endif

    }

    void StartPull()
    {
        if (_cancelLassoAction) { return; }
        UpdateState(PlayerState.PULL);

        _lassoHitObjectRigidBody.isKinematic = true;

        _lassoRenderer.Pullin(lassoThrowPosition, _lassoHitObjectTransform);
        Invoke("StartHold", timeToPullObject);

        SoundManager.Instance().PlaySFX("LassoWindUp");


        _accumPullTime = 0.0f;
    }
    void Pull()
    {
        _accumPullTime += Time.deltaTime;
        float percentageComplete = Mathf.Clamp(_accumPullTime / timeToPullObject, 0.0f, 1.0f);
        _lassoHitObjectRigidBody.transform.position = percentageComplete
            * Vector3.Distance(transform.position + transform.up * holdHeight, _lassoHitPointTransform.position)
            * (transform.position + transform.up * holdHeight - _lassoHitPointTransform.position).normalized + _lassoHitPointTransform.position;
    }

    void GetPullInput()
    {
#if !UNITY_IOS && !UNITY_ANDROID

        if (Input.GetKeyUp(lassoKey))
        {
            _cancelLassoAction = true;
            _lassoSelectedObject.currentlyLassoed = false;
            _lassoHitObjectRigidBody.isKinematic = false;
            _lassoHitObjectRigidBody.AddForce((transform.position - _lassoHitPointTransform.position).normalized * tossForwardImpulse * 0.5f,
                ForceMode.Impulse);
            UpdateState(PlayerState.IDLE);
        }
#endif
    }

    void StartHold()
    {
        if (_cancelLassoAction) { return; }
        UpdateState(PlayerState.HOLD);

        playerUI.ShowThrowBar();

        SoundManager.Instance().PlaySFX("LassoSpin");


        _accumHoldTime = 0.0f;
    }

    void Hold()
    {
        // Display UI rectangle for variable throw range
        _accumHoldTime += Time.deltaTime;
        _lassoHitObjectRigidBody.transform.position = transform.position 
            + _gravityObject.gravityOrientation.right * Mathf.Cos(_accumHoldTime * holdSwingSpeed) * holdSwingRadius
            + _gravityObject.gravityOrientation.forward * Mathf.Sin(_accumHoldTime * holdSwingSpeed) * holdSwingRadius
            + _gravityObject.gravityOrientation.up * holdHeight;

        playerUI.SetThrowIndicatorPos(Mathf.Sin(_accumHoldTime * lassoIndicatorSpeed));
    }

    void GetHoldInput()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
#else

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
#endif
        _moveInput = new Vector3(horizontal, 0, vertical).normalized;
        // Start the toss of the enemy based on the UI rectangle power ended on
#if !UNITY_IOS && !UNITY_ANDROID

        if (Input.GetKeyUp(lassoKey))
        {
            StartToss();
        }
#endif
    }

    void StartToss()
    {
        ThrowStrength strength = playerUI.GetThrowIndicatorStrength();
        print("Toss force was: " + strength);
        SoundManager.Instance().StopSFX("LassoSpin");


        float forceMult = 0.4f;
        switch(strength)
        {
            case ThrowStrength.MEDIUM:
                forceMult = 0.7f; break;
            case ThrowStrength.STRONG: 
                forceMult = 1.0f; break;
        }

        // change enemies state different so it hurts the boss
        (_lassoSelectedObject as LassoableEnemy).thrown = true;
        _lassoSelectedObject.currentlyLassoed = false;
        _lassoHitObjectRigidBody.isKinematic = false;
        _lassoHitObjectRigidBody.AddForce((_cameraTransform.forward * tossForwardImpulse + _gravityObject.gravityOrientation.up * tossUpwardImpulse) * forceMult,
            ForceMode.Impulse);

        UpdateState(PlayerState.TOSS);


        Invoke("EndToss", timeToToss);
    }

    void EndToss()
    {
        
        UpdateState(PlayerState.IDLE);

        playerUI.HideThrowBar();
    }

    void GetTossInput()
    {
#if UNITY_IOS || UNITY_ANDROID
        if (joystick != null)
        {
            horizontal = joystick.Horizontal;
            vertical = joystick.Vertical;
        }
#else

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
#endif
        _moveInput = new Vector3(horizontal, 0, vertical).normalized;
        if (_moveInput.magnitude > 0 || Input.anyKeyDown)
        {
            EndToss();
        }
    }



    void StartJump()
    {
        _rigidBody.AddForce(jumpImpulseForce * _gravityObject.gravityOrientation.up, ForceMode.Impulse);
        _gravityObject.gravityMult = 1.0f;
        SoundManager.Instance().PlaySFX("PlayerJump");
    }

    void EndJump()
    {
        _gravityObject.gravityMult = gravIncreaseOnJumpRelease;
        _jumpBufferTimer = 0;
    }

    void OnLand()
    {
        _detectLanding = false;
        SoundManager.Instance().PlaySFX("PlayerLand");
    }

    public void Damage(int damageAmount, Vector3 knockback)
    {
        if (health > 0 && _canTakeDamage)
        {
            if (damageAmount > 0)
            {
                ScreenShakeManager.Instance.ShakeCamera(2, 1, 0.1f);
                health -= damageAmount;
                playerUI.ChangeHealth(-damageAmount);
                SoundManager.Instance().PlaySFX("PlayerHurt");
            }
            ApplyKnockback(knockback);
            if (health <= 0)
            {
                // TODO: Play death animation and invoke after animation is complete
                Invoke("Respawn", 1.3f);
            }
            else
            {
                StartCoroutine(Invincibility());
                if (damageColor != null && normalColor != null && characterRender != null)
                {
                    StartCoroutine(FlashInvincibility(characterRender.GetComponent<Renderer>()));
                }
            }
        }
    }

    void Respawn()
    {
        SoundManager.Instance().StopAllSFX();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator FlashInvincibility(Renderer charRender)
    {
        while (!_canTakeDamage)
        {
            charRender.material = damageColor; // Assign damageColor material
            yield return new WaitForSeconds(0.2f);
            charRender.material = normalColor; // Assign normalColor material
            yield return new WaitForSeconds(0.2f); // Add a slight delay before looping again
        }
    }

    IEnumerator Invincibility()
    {
        _canTakeDamage = false;
        yield return new WaitForSeconds(2f);
        _canTakeDamage = true;
    }

    public void ApplyKnockback(Vector3 knockback)
    {
        GetComponent<Rigidbody>().AddForce(knockback, ForceMode.Impulse);
    }

    public void DisableForCutscene()
    {
        _disableForCutscene = true;
        UpdateState(PlayerState.IDLE);
        _moveInput = Vector3.zero;
        playerUI.HideUIForCutscene();
        GetComponent<PlayerCameraController>().DisableForCutscene();
    }

    public void EnableAfterCutscene()
    {
        _disableForCutscene = false;
        playerUI.ShowUIPostCutscene();
        GetComponent<PlayerCameraController>().EnableAfterCutscene();
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
    public float wiggleness;
    public float lassoRadius;
    public LayerMask swingLayerMask;

    private Transform _start;
    private Transform _target;
    private float _timeToHitTarget;
    private float _accumThrowTime;
    private float _accumHoldTime;

    public void Throw(Transform start, Transform target, float timeToHitTarget)
    {
        _start = start;
        _target = target;
        _timeToHitTarget = timeToHitTarget;
        lineRenderer.positionCount = lassoLoopSegments + lassoRopeSegments;
        _accumThrowTime = 0;
    }

    public void RenderThrow()
    {
        if (lineRenderer.positionCount == 0) { return; }

        // Calculations for current rope length and basis for rope
        _accumThrowTime += Time.deltaTime;
        float currentLength = (_accumThrowTime / _timeToHitTarget) * Vector3.Distance(_start.position, _target.position);
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
                // pos += EaseInElastic(percentAlongLine) * amplitude * _up * Mathf.Sin(_accumThrowTime * wiggleSpeed);
                pos += Mathf.Sin(_accumThrowTime * wiggleSpeed) * amplitude * Mathf.Sin(percentAlongLine * wiggleness) * percentAlongLine * _up;
            }
            lineRenderer.SetPosition(i, pos);
        }

        // Rendering lasso loop
        Vector3 lassoCenterPos = _start.position + (currentLength + lassoRadius) * _forward;

        for (int i = 0; i < lassoLoopSegments; i++)
        {
            float theta = ((float) i /  (float) lassoLoopSegments) * Mathf.PI * 2;
            Vector3 pos = lassoCenterPos
                + Mathf.Cos(theta) * -lassoRadius * (_accumThrowTime / _timeToHitTarget) * _forward
                + Mathf.Sin(theta) * lassoRadius * (_accumThrowTime / _timeToHitTarget) * _right
                + Mathf.Cos(_accumThrowTime * wiggleSpeed) * 0.2f * _right
                + Mathf.Sin(_accumThrowTime * wiggleSpeed) * 0.2f * _forward
                + Mathf.Cos((_accumThrowTime / _timeToHitTarget) * wiggleSpeed * theta / 10) * 0.5f * _up;

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

    float EaseOutElastic(float num)
    {
        const float constant = (2 * Mathf.PI) / 3;
        if (num == 0.0f)
        {
            return 0.0f;
        } 
        else if (num == 1.0f)
        {
            return 1.0f;
        } 
        else
        {
            return Mathf.Pow(2, -10 * num) * Mathf.Sin((num * 10 - 0.75f) * constant) + 1;
        }
    }

    float EaseInElastic(float num)
    {
        const float constant = (2 * Mathf.PI) / 3;
        if (num == 0.0f)
        {
            return 0.0f;
        }
        else if (num == 1.0f)
        {
            return 1.0f;
        } 
        else
        {
            return -Mathf.Pow(2, 10 * num - 10) * Mathf.Sin((num * 10 - 10.75f) * constant);
        }
    }

    public void RenderSwing(Transform swingObjectTransform)
    {
        if (lineRenderer.positionCount == 0) { return; }

        Vector3 knotPoint;
        Vector3 center = swingObjectTransform.position;
        Vector3 forward = (_start.position - center).normalized;
        Vector3 right = Vector3.Cross(Vector3.Cross(forward, playerTransform.forward).normalized, forward).normalized;

        WrapAroundObject(center, forward, right, (_target.position - center).magnitude * 0.2f, out knotPoint);

        float length = Vector3.Distance(_start.position, knotPoint);
        Vector3 dir = (knotPoint - _start.position).normalized;

        // Render the lasso rope
        for (int i = 0; i < lassoRopeSegments; i++)
        {
            float percentageAlongRope = ((float) i / (float) lassoRopeSegments);
            Vector3 pos = percentageAlongRope * length * dir + _start.position;
            lineRenderer.SetPosition(i, pos);
        }


    }

    public void Pullin(Transform start, Transform target)
    {
        _start = start;
        _target = target;

        _accumHoldTime = 0;
    }

    public void RenderPullin(Transform pulledObject)
    {
        float length = Vector3.Distance(_start.position, pulledObject.position);
        Vector3 dir = (pulledObject.position - _start.position).normalized;
        for (int i = 0; i < lassoRopeSegments; i++)
        {
            float percentageAlongRope = ((float)i / (float)lassoRopeSegments);
            Vector3 pos = percentageAlongRope * length * dir + _start.position;
            lineRenderer.SetPosition(i, pos);
        }

        WrapAroundObject(pulledObject.position, pulledObject.forward, pulledObject.right, 0.1f);
    }

    public void RenderHoldObject(Transform heldObject)
    {
        Vector3 knotPoint;
        WrapAroundObject(heldObject.position, heldObject.forward, heldObject.right, 0.1f, out knotPoint);

        float length = Vector3.Distance(_start.position, heldObject.position);
        Vector3 dir = (knotPoint - _start.position).normalized;
        Vector3 right = (knotPoint - heldObject.position).normalized;
        for (int i = 0; i < lassoRopeSegments; i++)
        {
            float percentageAlongRope = ((float)i / (float)lassoRopeSegments);
            Vector3 pos = percentageAlongRope * length * dir + _start.position;
                //+ right * EaseInElastic(percentageAlongRope) * 5.0f * Mathf.Sin(_accumHoldTime);
            lineRenderer.SetPosition(i, pos);
        }

        
        _accumHoldTime += Time.deltaTime;
    }

    public void RenderTossEnemy()
    {
        // ?
    }

    void WrapAroundObject(Vector3 center, Vector3 forward, Vector3 right, float wrapMult)
    {
        Vector3 temp;
        WrapAroundObject(center, forward, right, wrapMult, out temp);
    }

    void WrapAroundObject(Vector3 center, Vector3 forward, Vector3 right, float wrapMult, out Vector3 firstPoint)
    {
        firstPoint = Vector3.zero;
        for (int i = 0; i < lassoLoopSegments; i++)
        {
            float theta = ((float)i / (float)lassoLoopSegments) * (Mathf.PI * 2 + 0.2f);
            Vector3 exteriorPos = center
                    + Mathf.Cos(theta) * 5f * forward
                    + Mathf.Sin(theta) * 5f * right;

            Vector3 toCenter = (center - exteriorPos).normalized;
            Vector3 pos;
            RaycastHit hit;
            if (Physics.Raycast(exteriorPos, toCenter, out hit, 20.0f, swingLayerMask, QueryTriggerInteraction.Ignore))
            {
                pos = hit.point
                    + Mathf.Cos(theta) * wrapMult * forward
                    + Mathf.Sin(theta) * wrapMult * right;
            }
            else
            {
                // An Estimation since we didn't hit the mesh
                pos = center 
                    + Mathf.Cos(theta) * wrapMult * forward
                    + Mathf.Sin(theta) * wrapMult * right;
            }
            if (i == 0)
            {
                firstPoint = pos;
            }

            lineRenderer.SetPosition(i + lassoRopeSegments, pos);
        }
    }

    public void StopRendering()
    {
        lineRenderer.positionCount = 0;
    }
}
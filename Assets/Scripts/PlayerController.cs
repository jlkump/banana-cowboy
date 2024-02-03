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
    public float walkSpeed = 8.0f; // The max walk speed
    public float runSpeed = 12.0f; // The max run speed
    public float accelerationRate = (50f * 0.5f) / 8.0f;  // The rate of speed increase
    public float deccelerationRate = (50f * 0.5f) / 8.0f; // The rate of speed decrease (when no input is pressed)
    public float accelAirControlRatio = 0.8f;       // The ability for the player to re-orient themselves in the air while accelerating
    public float deccelAirControlRatio = 0.8f;      // The ability for the player to move themselves while in the air and deccelerating (range [0.0,1.0])
    public float jumpImpulseForce = 10.0f;
    public bool conserveMomentum = true;

    //private float _currentSpeed = 0.0f;
    private Vector3 _previousVel = Vector3.zero;
    private Vector3 _moveInput;
    private float _lastTimeOnGround = 0.0f;
    private bool _isRunning = false;

    [Header("Input")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;

    /*
    public Transform playerRoot;
    public Transform modelTransform;
    public Transform lassoThrowPos;
    public Animator playerAnimation;
    public LineRenderer lr;
    //public LayerMask swingable;

    //public Transform temp;


    [Header("Movement")]
    public float max_walk_speed = 8.0f;
    public float max_run_speed = 12.0f;
    public float accel_rate = (50.0f * 0.5f) / 15.0f;
    public float decel_rate = (100.0f * 0.5f) / 15.0f;
    public float accel_in_air_rate = 0.8f;
    public float decel_in_air_rate = 0.8f;
    public float jump_impulse_force = 10.0f;
    public float gravity_mult_on_jump_release = 3.0f;
    public bool conserve_momentum = true;
    public float dash_force;
    private bool can_Dash = true;
    private float dash_Cooldown = 0.6f;
    private float dash_Timer = 0.0f;
    private bool wasInAir = false; // Keep track of the previous state

    public float LastOnGroundTime { get; private set; }

    [Header("Buffer System")]
    public float jump_hold_buffer = 0.3f;
    private float jump_hold_buffer_timer;
    public float jump_buffer = 0.1f;
    private float jump_buffer_timer;

    [Header("Lasso")]
    // public float lasso_length = 15f; // TODO: Use this?
    public float lasso_reach_distance = 25f;
    public float lasso_dectection_distance = 90f;
    public float lasso_throw_force = 20.0f;
    public GameObject lasso_scope_indicator;
    public int max_number_of_indicators = 10;

    private GameObject lasso_target; // Null when no valid target. Targets are swingable, enemy, and collectable

    [Header("Lasso Rendering")]
    public Color targeted_color = Color.green;
    public Color out_of_range_color = Color.red;
    public Color in_range_color = Color.grey;
    private List<GameObject> indicators;

    public int lasso_num_lr_wrap_joints = 4;
    private Vector3 lasso_lr_end_pos;
    private List<Vector3> lasso_rope_wrap_positions;

    [Header("Swinging")]
    private SpringJoint swing_joint;
    private Vector3 current_rope_end_pos;

    [Header("Input")]
    public KeyCode lassoKey = KeyCode.Mouse0;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    // Having as serialized field messes up pause menu
    public GameManager gameManager;

    private Vector3 _moveInput;

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
    

    enum LassoState
    {
        NONE,
        WOUND_UP,
        SWING,
        ENEMY_HOLD,
        ENEMY_AIM
    }
    private LassoState lasso_state = LassoState.NONE;
    private LassoableEnemy held_enemy = null;
    */
    enum PlayerState
    {
        IDLE,
        AIR,
        WALK,
        RUN,
    };

    private PlayerState _state = PlayerState.AIR;
    

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _gravityObject = GetComponent<GravityObject>();

    }

    void Start()
    {
        _cameraTransform = Camera.main.transform;
        SetupLasso();



        /*
        jump_hold_buffer_timer = 0.0f;
        jump_buffer_timer = 0.0f;
        */

        /*
        soundManager = GameObject.Find("Sound Manager").GetComponent<SoundManager>();
        ui = GameObject.Find("Player UI").GetComponent<UIManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        */
    }
    void Update()
    {
        GetMoveInput();
        //GetLassoInput();
        //GetDashInput();
        /*
        jump_hold_buffer_timer -= Time.deltaTime;
        jump_buffer_timer -= Time.deltaTime;
        */

    }

    void FixedUpdate()
    {
        Run();
        UpdateLasso();
    }
    private void LateUpdate()
    {
        DrawRope();
    }

    void UpdateState(PlayerState newState)
    {
        if (_state != newState)
        {
            // Signal update to animation
            UpdateAnimState();
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

    void SetupLasso()
    {
        /*
        indicators = new List<GameObject>(max_number_of_indicators);
        for (int i = 0; i < max_number_of_indicators; i++)
        {
            GameObject indicator = Instantiate(lasso_scope_indicator, transform.position, Quaternion.identity, GameObject.Find("Player UI").transform);
            indicator.SetActive(false); // Should make invisible, we shall see
            indicators.Add(indicator);
        }
        lasso_target = null;
        lasso_rope_wrap_positions = new List<Vector3>();
        */
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

    void GetLassoInput()
    {
        /*

        if (Input.GetKeyDown(lassoKey) && !gameManager.pauseMenu.activeSelf)
        {
            switch (lasso_state)
            {
                case LassoState.NONE:
                    print("lassoing windup!");
                    soundManager.PlaySFX(ropeWindUpSFX, 1);
                    StartLassoWindup();
                    playerAnimation.speed = 1.0f; // CHANGE LATER: only because there's only one run/walk animation

                    playerAnimation.Play("New Layer.BananaCowboyLassoWindUp");
                    break;
                case LassoState.WOUND_UP:
                case LassoState.SWING:
                    print("lasso end swing");

                    EndSwing();
                    break;
                case LassoState.ENEMY_HOLD:
                    AimLassoEnemy();
                    print("Lasso aim enemy");
                    playerAnimation.Play("New Layer.BananaCowboyLassoWindUp");
                    break;
                default:
                    // Shouldn't be possible
                    print("Click down on invalid lasso state");
                    break;
            }
        }

        if (Input.GetKeyUp(lassoKey))
        {

            switch (lasso_state)
            {

                case LassoState.WOUND_UP:
                    soundManager.StopSFX();
                    print("lasso windup release");
                    EndLassoWindup();
                    playerAnimation.Play("New Layer.BananaCowboyIdle");
                    break;
                case LassoState.ENEMY_AIM:
                    ThrowLassoEnemy();
                    print("Lasso release of enemy");
                    playerAnimation.Play("New Layer.BananaCowboyIdle");
                    break;
                case LassoState.SWING:
                case LassoState.NONE:
                    break;
                default:
                    // Shouldn't be possible
                    print("Click release on invalid lasso state");
                    break;
            }
        }
        */
    }

    void UpdateLasso()
    {
        /*
        if (lasso_state == LassoState.WOUND_UP)
        {
            LassoWindup();
        }
        else if (lasso_state == LassoState.ENEMY_HOLD)
        {
            if (!soundManager.soundEffectObject.clip.name.Contains("Spinning"))
            {
                soundManager.PlaySFX(ropeSpinningEnemySFX, 1);
            }
            HoldLassoEnemy();
        }
        else if (lasso_state == LassoState.ENEMY_AIM)
        {
            soundManager.StopSFX();
            AimLassoEnemy();
        }
        */
    }

    void GetDashInput()
    {
        /*
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
         */
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
        //print("Move force is " + movement);
        //print("Current velocity " + _rigidBody.velocity);
        _rigidBody.AddForce(movement);

        // Spin player model and orientation to right direction to face
        if (_moveInput.magnitude > 0 && model != null)
        {
            model.rotation = Quaternion.Slerp(model.rotation, Quaternion.LookRotation(targetVelocity.normalized, transform.up), Time.deltaTime * 8);
        }
    }

    void StartLassoWindup()
    {
        /*
        lasso_state = LassoState.WOUND_UP;

        current_rope_end_pos = lassoThrowPos.position;
        lasso_lr_end_pos = transform.position + transform.up;
        lr.positionCount = 2;
        */
    }

    void LassoWindup()
    {
        /*
        Collider[] colliders = Physics.OverlapSphere(transform.position, lasso_dectection_distance);

        List<Vector3> indicator_positions = new List<Vector3>();

        current_rope_end_pos = lassoThrowPos.position;
        lasso_lr_end_pos = transform.position + transform.up;

        Vector3 closest_point = Vector3.zero;
        float closest_distance = float.MaxValue;
        Collider closest_target_hit = null;

        for (int i = 0; i < colliders.Length; i++)
        {
            Collider collider = colliders[i];
            if (collider.gameObject.GetComponent<LassoableEnemy>() != null || collider.gameObject.GetComponent<Swingable>() != null)
            {
                // Note location of object for the indicators

                Vector3 viewport_point = Camera.main.WorldToViewportPoint(collider.transform.position);
                // Check if we are in the viewport, if we are, then add the collider positions for indicators.
                // Also mark the closest hit
                if (viewport_point.x > 0 && viewport_point.y > 0 &&
                    viewport_point.x < 1.0 && viewport_point.y < 1.0 &&
                    viewport_point.z > 0.0)
                {
                    indicator_positions.Add(collider.transform.position);
                    Vector3 viewport_point_centered = viewport_point - new Vector3(0.5f, 0.5f, 0.0f);
                    viewport_point_centered.z = 0;
                    if (viewport_point_centered.magnitude < closest_distance)
                    {
                        closest_target_hit = collider;
                        closest_distance = viewport_point_centered.magnitude;
                        closest_point = viewport_point;
                    }
                }
            }
        }

        // Place indicators for windup
        // 1. First, deactivate all previous indicators
        // 2. Then, activate all indicators that are necessary

        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
        }
        int indicator_index = 0; // This is used to assign indicators. If it is ever >= indicators.Count, then we just stop doing indicators
        for (int i = 0; i < indicator_positions.Count; i++)
        {
            Vector3 viewport_point = Camera.main.WorldToViewportPoint(indicator_positions[i]);
            if (indicator_index < indicators.Count)
            {
                GameObject current_indicator = indicators[indicator_index];
                current_indicator.SetActive(true);
                current_indicator.transform.position = Camera.main.WorldToScreenPoint(indicator_positions[i]);
                //current_indicator.GetComponent<Image>().enabled = true;
                indicator_index++;
                // The indicator is in view, so place an indicator object on it
                if (Vector3.Distance(indicator_positions[i], transform.position) < lasso_reach_distance)
                {
                    if (viewport_point == closest_point)
                    {
                        // The lasso object is within range and the closest target, show a green indicator
                        current_indicator.GetComponent<Image>().color = targeted_color;
                    }
                    else
                    {
                        // The lasso object is within range, place a grey indicator if not the closest target
                        current_indicator.GetComponent<Image>().color = in_range_color;
                    }

                    if (closest_target_hit == null)
                    {
                        lasso_target = null;
                    }
                    else
                    {
                        lasso_target = closest_target_hit.gameObject;
                    }
                }

                if (Vector3.Distance(indicator_positions[i], transform.position) > lasso_reach_distance)
                {
                    // The lasso object is outside range, place a red indicator
                    current_indicator.GetComponent<Image>().color = out_of_range_color;
                }

                if (closest_target_hit == null)
                {
                    lasso_target = null;
                }
                else
                {
                    lasso_target = closest_target_hit.gameObject;
                }
            }
        }
        */
    }

    void EndLassoWindup()
    {
        /*
        if (lasso_target != null)
        {
            soundManager.PlaySFX(ropeThrowSFX, 1);

            if (lasso_target.GetComponent<LassoableEnemy>() != null)
            {
                GrabLassoEnemy(lasso_target.GetComponent<LassoableEnemy>());
            }
            else if (lasso_target.GetComponent<Swingable>() != null)
            {
                StartSwing(lasso_target.transform.position);
            }
        }
        else
        {
            lasso_state = LassoState.NONE;
        }

        // Disable all indicators
        for (int i = 0; i < indicators.Count; i++)
        {
            indicators[i].SetActive(false);
        }
        */
    }

    void StartSwing(Vector3 swing_position)
    {
        /*
        lasso_state = LassoState.SWING;
        lasso_lr_end_pos = swing_position;
        swing_joint = playerRoot.gameObject.AddComponent<SpringJoint>();
        swing_joint.autoConfigureConnectedAnchor = false;
        swing_joint.connectedAnchor = swing_position;

        float distance_from_point = Vector3.Distance(playerRoot.position, swing_position);

        swing_joint.minDistance = distance_from_point * 0.6f;
        swing_joint.maxDistance = distance_from_point * 0.25f;

        current_rope_end_pos = lassoThrowPos.position;

        swing_joint.spring = 4.5f;
        swing_joint.damper = 7f;
        swing_joint.massScale = 4.5f;

        lr.positionCount = 2;
        */
    }

    void EndSwing()
    {
        /*
        lasso_state = LassoState.NONE; // Will be corrected on next update if wrong
        lr.positionCount = 0;
        Destroy(swing_joint);
        */
    }

    void GrabLassoEnemy(LassoableEnemy enemy)
    {
        /*
        lasso_state = LassoState.ENEMY_HOLD;
        enemy.SetLassoActor(transform);
        held_enemy = enemy;
        current_rope_end_pos = lassoThrowPos.position;
        lasso_lr_end_pos = held_enemy.transform.position;
        lr.positionCount = 2;
        */
    }

    void HoldLassoEnemy()
    {
        /*
        // Might need nothing here, but here just in case
        current_rope_end_pos = held_enemy.transform.position;
        */
    }

    void AimLassoEnemy()
    {
        /*
        // TODO: Trajectory prediction
        current_rope_end_pos = held_enemy.transform.position;
        lasso_state = LassoState.ENEMY_AIM;
        */
    }

    void ThrowLassoEnemy()
    {
        /*
        // TODO: Release enemy
        held_enemy.ThrowEnemyInDirection(cameraTransform.forward + transform.up * 0.2f, lasso_throw_force);
        lasso_state = LassoState.NONE;
        lr.positionCount = 0;
        */
    }

    void DrawRope()
    {
        /*
        if (lasso_state == LassoState.NONE)
        {
            return;
        }

        if (lasso_state == LassoState.SWING)
        {
            current_rope_end_pos = Vector3.Lerp(current_rope_end_pos, lasso_lr_end_pos, Time.deltaTime * 8.0f);
        }

        lr.SetPosition(0, lassoThrowPos.position);
        lr.SetPosition(1, current_rope_end_pos);

        for (int i = 0; i < lasso_rope_wrap_positions.Count(); i++)
        {
            lr.SetPosition(2 + i, lasso_rope_wrap_positions[i]);
        }
        */
    }

    void StartJump()
    {
        _rigidBody.AddForce(jumpImpulseForce * transform.up, ForceMode.Impulse);
        _gravityObject.gravityMult = 1.0f;
        /*
        //_is_jumping = true;
        soundManager.PlaySFX(jumpSFX, 1);
        GetComponent<GravityObject>().gravity_mult = 1.0f;
        GetComponent<Rigidbody>().AddForce(transform.up * jump_impulse_force, ForceMode.Impulse);
        jump_hold_buffer_timer = jump_hold_buffer;
        jump_buffer_timer = 0;
        */

    }

    void EndJump()
    {
        print("Jump end");
        _gravityObject.gravityMult = 3.0f;
        /*
        //_is_jumping = false;
        jump_buffer_timer = 0;
        //Vector3 velocity = GetComponent<Rigidbody>().velocity;
        //Vector3 up = Vector3.Project(velocity, transform.up);
        //float mult = (Vector3.Dot(up, transform.up) > 0 && jump_hold_buffer_timer > 0) ? 0.5f : 1;
        GetComponent<GravityObject>().gravity_mult = gravity_mult_on_jump_release;
        */
    }

    void Dash()
    {
        /*
        //GetComponent<Rigidbody>().AddForce(temp.transform.forward * dash_force, ForceMode.Impulse);
        soundManager.PlaySFX(dashSFX, 1);
        GetComponent<Rigidbody>().AddForce(modelTransform.forward * dash_force, ForceMode.Impulse);
        */
    }
}

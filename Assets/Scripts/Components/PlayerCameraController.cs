using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    CinemachineVirtualCamera _cinemachineCamController;
    [SerializeField]
    Transform _cameraPivot = null, _cameraTarget = null, _cameraCurrent = null;


    [SerializeField, Range(1.0f, 80.0f)]
    float _orbitRotationSpeed = 10.0f, _tiltRotationSpeed = 10.0f, _orbitZoomSpeed = 10.0f;

    [SerializeField, Range(1.0f, 20.0f)]
    float _reorientSpeed = 8.0f;

    [SerializeField, Range(0f, 30f)]
    float _orbitMinDist = 3f, _orbitMaxDist = 20f, _orbitDistance = 10f;

    public bool invertY = true, invertZoom = false;
    float mouseX, mouseY;

    private int _validTouchID = -1;
    public static int uiTouching;

    void Start()
    {
        // Removes cursor from screen and keeps it locked to center
        HideCursor();
    }

    void Update()
    {
        if (_cameraTarget == null || _cameraPivot == null) { return; }
        if (!PauseManager.pauseActive)
        {
#if !UNITY_IOS && !UNITY_ANDROID
            mouseX = Input.GetAxisRaw("Mouse X");
            mouseY = Input.GetAxisRaw("Mouse Y");
            GetRotationInput();
#else
            CameraMovementMobile();
#endif
            BlendToTarget();
        }
    }

    void CameraMovementMobile()
    {
        if (uiTouching == Input.touchCount) // Prevents unneccesary looping
        {
            return;
        }

        // Loop through all touches
        foreach (Touch touch in Input.touches)
        {
            // Check if it's the beginning phase of touch
            if (touch.phase == TouchPhase.Began)
            {
                // Check if the touch is not touching a UI element
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    // Assign the valid touch ID
                    _validTouchID = touch.fingerId;
                }
            }

            // Check if it's the end phase of touch
            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                // Check if the ended/canceled touch matches the valid touch ID
                if (touch.fingerId == _validTouchID)
                {
                    // Reset the valid touch ID
                    _validTouchID = -1;
                }
            }

            // Check if the current touch is the valid one
            if (touch.fingerId == _validTouchID)
            {
                // Perform camera movement based on touch delta
                Vector2 touchDeltaPosition = touch.deltaPosition * 0.02f;

                // Adjust rotation based on touch delta
                mouseX = touchDeltaPosition.x;
                mouseY = touchDeltaPosition.y;
                GetRotationInput();
            }
        }
    }



    void GetRotationInput()
    {
        float mouseScroll = -Input.GetAxisRaw("Mouse ScrollWheel");

        if (invertY)
        {
            mouseY = -mouseY;
        }
        if (invertZoom)
        {
            mouseScroll = -mouseScroll;
        }

        _cameraPivot.localRotation *= Quaternion.AngleAxis(mouseX * _orbitRotationSpeed, Vector3.up);
        _cameraTarget.localRotation *= Quaternion.AngleAxis(mouseY * _tiltRotationSpeed, Vector3.right);

        var angles = _cameraTarget.localEulerAngles;
        angles.z = 0;
        angles.y = 0;

        var angle = _cameraTarget.localEulerAngles.x;

        if (angle < 180 && angle > 60)
        {
            angles.x = 60;
        }
        else if (angle > 60 && angle < 290)
        {
            angles.x = 290;
        }

        _cameraTarget.localEulerAngles = angles;

        _orbitDistance = Mathf.Clamp(_orbitDistance + mouseScroll * _orbitZoomSpeed, _orbitMinDist, _orbitMaxDist);
        if (_cinemachineCamController != null)
        {
            Cinemachine3rdPersonFollow ccb = _cinemachineCamController.GetCinemachineComponent<Cinemachine3rdPersonFollow>();
            if (ccb != null)
            {
                ccb.CameraDistance = _orbitDistance;
            }
        }
    }

    void BlendToTarget()
    {
        _cameraCurrent.rotation = Quaternion.Slerp(_cameraCurrent.rotation, _cameraTarget.rotation, Time.deltaTime * _reorientSpeed);
        _cameraCurrent.position = Vector3.Lerp(_cameraCurrent.position, _cameraTarget.position, Time.deltaTime * _reorientSpeed);
    }

    public static void HideCursor()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
#endif
    }

    public static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    public void DisableForCutscene()
    {
        if (_cinemachineCamController != null)
        {
            _cinemachineCamController.gameObject.SetActive(false);
        }
    }

    public void EnableAfterCutscene()
    {
        if (_cinemachineCamController != null)
        {
            _cinemachineCamController.gameObject.SetActive(true);
        }
    }
}

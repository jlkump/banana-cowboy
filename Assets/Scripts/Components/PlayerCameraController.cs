using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    void Start()
    {
        // Removes cursor from screen and keeps it locked to center
        HideCursor();
    }

    void Update()
    {
        if (_cameraTarget == null || _cameraPivot == null) { return; }
        if (!PauseManager.pauseActive) {
            GetRotationInput();
            BlendToTarget();
        }
    }

    void GetRotationInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");
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
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    public static void ShowCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }
}

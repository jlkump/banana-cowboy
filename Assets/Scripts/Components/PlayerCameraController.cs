using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTarget;
    public float orbitRotationSpeed = 10.0f;
    public float tiltRotationSpeed = 10.0f;
    [Range(-10.0f, 10.0f)]
    public float targetHeight = 3.0f;

    public float reorientTime = 0.4f;
    private float _accumReorientTime = 0.0f;
    private Vector3 _reorientUp = Vector3.up;

    private Vector3 _camUp = Vector3.up;
    private Vector3 _camForward = Vector3.forward;
    private float _tiltDegrees = 0.0f;
    private float _orbitDegrees = 0.0f;


    void Start()
    {
        // Removes cursor from screen and keeps it locked to center
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    void Update()
    {
        if (cameraTarget == null) { return; }
        //if (_accumReorientTime < reorientTime)
        //{
        //    _accumReorientTime += Time.deltaTime;
        //    float t = Mathf.Clamp(_accumReorientTime / reorientTime, 0.0f, 1.0f);
        //    cameraTarget.rotation = Quaternion.Slerp(
        //        cameraTarget.rotation, 
        //        Quaternion.FromToRotation(cameraTarget.up, _reorientUp) * cameraTarget.rotation, 
        //        t
        //    );
        //    cameraTarget.position = Vector3.Lerp(cameraTarget.position, transform.position + _camUp * targetHeight, t);
        //} 
        //else
        //{
        //}
        GetRotationInput();
    }

    void GetRotationInput()
    {
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        _orbitDegrees += + mouseX * orbitRotationSpeed;
        if (_orbitDegrees < 0.0f)
        {
            _orbitDegrees += 360.0f;
        } 
        else if (_orbitDegrees > 360.0f)
        {
            _orbitDegrees -= 360.0f;
        }
        _tiltDegrees = (_tiltDegrees + mouseY * tiltRotationSpeed);
        if (_tiltDegrees < -30.0f)
        {
            _tiltDegrees = -30.0f;
        } 
        else if (_tiltDegrees > 60.0f)
        {
            _tiltDegrees = 60.0f;
        }

        cameraTarget.rotation = Quaternion.AngleAxis(_orbitDegrees, _camUp) * Quaternion.LookRotation(_camForward, _camUp);
        cameraTarget.rotation = Quaternion.AngleAxis(_tiltDegrees, cameraTarget.right) * cameraTarget.rotation;

        cameraTarget.position = transform.position + _camUp * targetHeight;
    }

    //void GetRotationInput()
    //{
    //    float mouseX = Input.GetAxisRaw("Mouse X");
    //    float mouseY = Input.GetAxisRaw("Mouse Y");

    //    // This code is used from Unity's tutorial on Cinemachine
    //    // https://www.youtube.com/watch?v=537B1kJp9YQ
    //    cameraTarget.rotation *= Quaternion.AngleAxis(mouseX * orbitRotationSpeed, cameraTarget.up);
    //    cameraTarget.rotation *= Quaternion.AngleAxis(mouseY * tiltRotationSpeed, cameraTarget.right);

    //    var angles = cameraTarget.localEulerAngles;
    //    angles.z = 0;

    //    var angle = cameraTarget.localEulerAngles.x;

    //    if (angle < 180 && angle > 50)
    //    {
    //        angles.x = 50;
    //    }
    //    else if (angle > 50 && angle < 340)
    //    {
    //        angles.x = 340;
    //    }

    //    cameraTarget.localEulerAngles = angles;
    //}

    public void SetNewUp(Vector3 up)
    {
        if (Vector3.Distance(cameraTarget.up, up) > 0.1f)
        {
            _accumReorientTime = 0.0f;
            _reorientUp = up;
            _camUp = up;
            _camForward = Vector3.Cross(up, Vector3.Cross(_camForward, up).normalized).normalized;
        }
    }
}

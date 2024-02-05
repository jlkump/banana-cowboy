using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTarget;
    public float horizontalRotationSpeed = 1.0f;
    public float verticalRotationSpeed = 1.0f;

    void Start()
    {
        // Removes cursor from screen and keeps it locked to center
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    void Update()
    {
        if (cameraTarget == null) { return; }
        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        // This code is used from Unity's tutorial on Cinemachine
        // https://www.youtube.com/watch?v=537B1kJp9YQ
        cameraTarget.rotation *= Quaternion.AngleAxis(mouseX * horizontalRotationSpeed, Vector3.up);
        cameraTarget.rotation *= Quaternion.AngleAxis(mouseY * verticalRotationSpeed, Vector3.right);

        var angles = cameraTarget.localEulerAngles;
        angles.z = 0;

        var angle = cameraTarget.localEulerAngles.x;

        if (angle < 180 && angle > 50)
        {
            angles.x = 50;
        } 
        else if (angle > 50 && angle < 340)
        {
            angles.x = 340;
        }

        cameraTarget.localEulerAngles = angles;
    }
}

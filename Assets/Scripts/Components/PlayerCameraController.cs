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
        print("Input is horizontal: " + mouseX + ", " + mouseY);
        cameraTarget.rotation *= Quaternion.AngleAxis(mouseX * horizontalRotationSpeed, Vector3.up);
        cameraTarget.rotation *= Quaternion.AngleAxis(mouseY * verticalRotationSpeed, Vector3.right);

        var angles = cameraTarget.localEulerAngles;
        angles.z = 0;

        var angle = cameraTarget.localEulerAngles.x;

        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        } 
        else if (angle > 180 && angle < 40)
        {
            angles.x = 40;
        }

        cameraTarget.localEulerAngles = angles;
    }
}

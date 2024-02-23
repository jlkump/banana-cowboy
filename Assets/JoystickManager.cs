using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoystickManager : MonoBehaviour
{
    public Canvas renderCamera;

    private void Start()
    {
        renderCamera.worldCamera = GameObject.Find("Camera").GetComponent<Camera>();
        renderCamera.planeDistance = 0.5f;
    }
}

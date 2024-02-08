using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    public Transform target;
    public int direction = 1;
    public float _angle = 0f;

    [SerializeField]
    private float _speed = 2f;
    public float _radius = 2f;

    private float x = 0f;
    private float y = 0f;
    private float z = 0f;

    // Update is called once per frame
    void Update()
    {
        x = Mathf.Cos(_angle);
        z = Mathf.Sin(_angle); 

        Vector3 offset = new Vector3(x, 0, z) * _radius;

        transform.position = target.position + offset;
        _angle += _speed * Time.deltaTime * direction;
    }

}

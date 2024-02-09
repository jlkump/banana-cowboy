using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircularMovement : MonoBehaviour
{
    [Header("Orange Slice Orbit")]
    public Transform target;
    public int direction = 1;
    public float angle = 0f;
    public float speed = 2f;
    public float radius = 5f;

    [Header ("Orange Slice Boomerang Animation")]
    public bool ifRotateY;
    public float rotationSpeedY;

    private float x = 0f;
    private float z = 0f;

    [Header ("Knockback")]
    Vector3 knockback = Vector3.zero;

    private void Start()
    {
        knockback = 15 * ((transform.position - target.position).normalized + Vector3.up);
    }
    // Update is called once per frame
    void Update()
    {
        x = Mathf.Cos(angle);
        z = Mathf.Sin(angle); 

        Vector3 offset = new Vector3(x, 0, z) * radius;

        transform.position = target.position + offset;
        angle += speed * Time.deltaTime * direction;

        if (ifRotateY)
        {
            float rotationAngle = rotationSpeedY * Time.deltaTime * direction;

            // Rotate the object around the y-axis of the target
            transform.RotateAround(target.position, Vector3.up, rotationAngle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerController>().Damage(1, knockback);
        }
    }

}

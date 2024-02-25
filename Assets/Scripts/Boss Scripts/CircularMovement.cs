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
    public float radius = 15f;

    [Header ("Orange Slice Boomerang Animation")]
    public bool ifRotateY;
    public float rotationSpeedY;

    private float x = 0f;
    private float z = 0f;
    public Material normalColor;
    public Material transparentColor;
    public Renderer model;

    [Header ("Knockback")]
    Vector3 knockback = Vector3.zero;
    public int flatKnockBack;

    private void Start()
    {
        knockback = flatKnockBack * ((transform.position - target.position).normalized + Vector3.up);
        model = transform.GetChild(0).GetComponent<Renderer>();
    }
    // Update is called once per frame
    void Update()
    {
        x = Mathf.Cos(angle);
        z = Mathf.Sin(angle); 

        Vector3 offset = new Vector3(x, 0, z) * radius;

        transform.position = target.position + offset;
        if (!target.GetComponentInParent<OrangeBoss>().indicating)
        {
            angle += direction * speed * Time.deltaTime;
        }

        if (ifRotateY)
        {
            float rotationAngle = direction * rotationSpeedY * Time.deltaTime;

            // Rotate the object around the y-axis of the target
            transform.RotateAround(target.position, Vector3.up, rotationAngle);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print(other.name);
            other.GetComponentInParent<PlayerController>().Damage(1, knockback);
        }
    }

    public void SetCollider(bool val)
    {
        GetComponent<BoxCollider>().enabled = val;
        if (val && model != null)
        {
            model.GetComponent<Renderer>().material = normalColor;
        }
        else
        {
            model.GetComponent<Renderer>().material = transparentColor;
        }
    }

}

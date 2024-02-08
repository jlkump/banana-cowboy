using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeBoss : MonoBehaviour
{
    public GameObject orangeSliceBoomerangs;
    public float spawnDistance = 2f;

    public GameObject temp;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Change the condition as needed
        {
            SpawnBoomerangs();
        }
    }

    void SpawnBoomerangs()
    {
        // Spawn boomerang to the right
        Vector3 spawnPositionRight = transform.position + transform.right * spawnDistance;
        GameObject boomerangRight = Instantiate(orangeSliceBoomerangs, spawnPositionRight, Quaternion.identity);
        boomerangRight.GetComponent<CircularMovement>().target = transform;

        // Spawn boomerang to the left
        Vector3 spawnPositionLeft = transform.position - transform.right * spawnDistance;
        GameObject boomerangLeft = Instantiate(orangeSliceBoomerangs, spawnPositionLeft, Quaternion.identity);
        boomerangLeft.GetComponent<CircularMovement>().direction = -1;
        boomerangLeft.GetComponent<CircularMovement>().target = transform;
        boomerangLeft.GetComponent<CircularMovement>()._angle = 180.0f * Mathf.Deg2Rad;
    }
}

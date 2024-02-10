using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LassoableEnemy : LassoObject
{
    public bool thrown = false;


    // Just using to test if the enemy is looking at player correctly
/*    private GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, target.transform.position, 10 * Time.deltaTime);
    }*/
}

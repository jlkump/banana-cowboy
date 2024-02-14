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

    private void OnCollisionStay(Collision collision)
    {
        // TODO: Handle orange, blueberry, strawberry enemies and bosses, and blender
        if (collision.collider.CompareTag("Boss"))
        {
            if (thrown)
            {
                // Will fix to handle more bosses (for orange, handle weak spots too)
                if (collision.transform.name == "Orange Boss" || collision.transform.parent.parent.name == "Orange Boss") {
                    if (collision.transform.name.Contains("Weak Spot"))
                    {
                        print("Weak Spot Damage");
                        collision.transform.parent.parent.gameObject.GetComponent<OrangeBoss>().Damage(2);
                    } 
                    else
                    {
                        print("Normal Damage");
                        collision.gameObject.GetComponent<OrangeBoss>().Damage(1);
                    }
                }
                Destroy(gameObject);
            }
        }
    }
}

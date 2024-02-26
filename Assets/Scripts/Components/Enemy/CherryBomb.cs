using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CherryBomb : LassoableEnemy
{
    public GameObject explosion;

   private void OnCollisionEnter(Collision collision)
    {
        if (thrown)
        {
            print("Boom");
            // Add sound here
            SpawnExplosion();
            Destroy(gameObject);
        }
    }
    void SpawnExplosion()
    {
        Instantiate(explosion, gameObject.transform.position, Quaternion.identity);
    }
}

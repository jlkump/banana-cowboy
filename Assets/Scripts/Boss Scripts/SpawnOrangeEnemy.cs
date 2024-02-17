using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnOrangeEnemy : MonoBehaviour
{

    public GameObject character;

    public void Spawn()
    {
        Instantiate(character, transform.position, transform.rotation);
        Destroy(gameObject);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeBoss : MonoBehaviour
{
    public GameObject orangeSliceBoomerangs;
    public GameObject minions;
    public BossStates state;

    public float boomerangCooldown = 8f;
    private float cooldownTimer;
    public enum BossStates
    {
        IDLE,BOOMERANG,PEEL,SPAWN,COOLDOWN
    };

    private void Start()
    {
        state = BossStates.IDLE;
    }

/*    void Update()
    {
        if (state == BossStates.IDLE) // Change the condition as needed
        {
            Debug.Log("STARTING ATTACK");
            state = BossStates.BOOMERANG;
        }
    }*/

    private void Update()
    {
        switch (state)
        {
            case BossStates.IDLE:
                state = BossStates.BOOMERANG;
                break;
            case BossStates.BOOMERANG:
                SpawnBoomerangs();
                break; 
            case BossStates.PEEL:
                break;
            case BossStates.SPAWN:
                SpawnEnemies();
                break;
            case BossStates.COOLDOWN:
                Cooldown();
                break;
            default:
                break;
        }
    }

    void SpawnBoomerangs()
    {
        // Add animation here

        // Spawn boomerang to the right
        Vector3 spawnPosition = transform.position;
        for (int i = 0; i < 4; i++)
        {
            GameObject boomerangRight = SpawnBoomerang(spawnPosition + transform.right, i);
            GameObject boomerangLeft = SpawnBoomerang(spawnPosition - transform.right, i);
            StartCoroutine(DestroyBoomerangs(boomerangRight, boomerangLeft));
        }
        cooldownTimer = 5f + boomerangCooldown;
        state = BossStates.COOLDOWN;
    }

    void SpawnEnemies()
    {
        // Add animation here

        // Spawn boomerang to the right
        Vector3 spawnPosition = transform.position;
        for (int i = 0; i < 4; i++)
        {
            Instantiate(minions);
        }
        cooldownTimer = 10f;
        state = BossStates.COOLDOWN;
    }

    private GameObject SpawnBoomerang(Vector3 position, int radiusAdd)
    {
        GameObject boomerang = Instantiate(orangeSliceBoomerangs, position, Quaternion.identity);
        CircularMovement circularMovement = boomerang.GetComponent<CircularMovement>();
        circularMovement.target = transform;
        circularMovement.direction = position.x > transform.position.x ? -1 : 1;
        circularMovement.angle = (position.x < transform.position.x ? 180f : 0f) * Mathf.Deg2Rad;
        circularMovement.radius += (radiusAdd * 5);
        return boomerang;
    }

    void Cooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            state = BossStates.IDLE;
        }
    }

    IEnumerator DestroyBoomerangs(GameObject x, GameObject y)
    {
        yield return new WaitForSeconds(boomerangCooldown);
        Destroy(x);
        Destroy(y);
    }
}

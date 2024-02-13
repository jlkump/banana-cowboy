using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OrangeBoss : MonoBehaviour
{
    [Header("Atacks")]
    public GameObject orangeSliceBoomerangs;
    public GameObject minions;
    public int numberOfEnemies;
    public float boomerangCooldown;
    private float cooldownTimer;
    private readonly int moves = 3;
    private int currMove;
    public bool indicating = false;
    public bool boomerangSpinning = false;

    public Animator animator;

    public GameObject[] spawnPoints;
    public GameObject origin;
    public GameObject player;

    //public float sizeOfArena;
    public BossStates state;

    [Header("Damage")]
    public int maxHealth;
    private int health;
    public Image healthUI;

    public enum BossStates
    {
        IDLE,BOOMERANG,PEEL,SPAWN,COOLDOWN
    };

    private void Start()
    {
        state = BossStates.IDLE;
        //state = BossStates.PEEL;

        health = maxHealth;
        currMove = 0;

        player = GameObject.FindWithTag("Player");
        indicating = false;
        boomerangSpinning = false;

    }

    private void Update()
    {
        if (player != null)
        {
            transform.LookAt(new Vector3 (player.transform.position.x, transform.position.y, player.transform.position.z));
        }

        switch (state)
        {
            case BossStates.IDLE:
                if (currMove%moves == 0)
                {
                    state = BossStates.BOOMERANG;
                }
                else if (currMove%moves == 1)
                {
                    state = BossStates.SPAWN;
                }
                else if (currMove % moves == 2)
                {
                    state = BossStates.PEEL;
                }
                break;
            case BossStates.BOOMERANG:
                SpawnBoomerangs();
                break; 
            case BossStates.PEEL:
                PeelSlam();
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

        if (Input.GetKeyDown(KeyCode.P))
        {
            Damage(1);
        }
    }

    void SpawnBoomerangs()
    {
        // Add animation here

        
        Vector3 spawnPosition = transform.position;
        for (int i = 0; i < 5; i++)
        {
            GameObject boomerangRight = SpawnBoomerang(spawnPosition + transform.right, i);
            GameObject boomerangLeft = SpawnBoomerang(spawnPosition - transform.right, i);
            StartCoroutine(DestroyBoomerangs(boomerangRight, boomerangLeft));
        }
        cooldownTimer =  3f + boomerangCooldown;
        state = BossStates.COOLDOWN;
        indicating = true;
        StartCoroutine(BoomerangStartup());
    }

    IEnumerator SpinningBoomerangs()
    {
        while (boomerangSpinning)
        {
            yield return new WaitForEndOfFrame();
            SoundManager.S_Instance().Play("OrangeBossBoomerangs");
        }
    }

    IEnumerator BoomerangStartup()
    {
        yield return new WaitForSeconds(2);
        indicating = false;
        boomerangSpinning = true;
        StartCoroutine(SpinningBoomerangs());
    }

    void SpawnEnemies()
    {
        // Add animation here

        SoundManager.S_Instance().Play("OrangeBossSummon");

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            /*Vector3 temp = UnityEngine.Random.onUnitSphere;
            Vector3 spawnPosition = transform.position + temp * UnityEngine.Random.Range(60, sizeOfArena);
            print(temp+", "+spawnPosition);
            spawnPosition.y = 0;*/

/*            int rand = UnityEngine.Random.Range(0, 2);
*/            
            Vector3 spawnPosition = spawnPoints[i].transform.position;
            Instantiate(minions, spawnPosition, transform.rotation);
        }
        cooldownTimer = 5f;
        state = BossStates.COOLDOWN;
    }

    void PeelSlam()
    {
        // add animation here
        //StartCoroutine(PeelSlamAnimation());
        cooldownTimer = 10f;
        state = BossStates.COOLDOWN;
    }

    IEnumerator PeelSlamAnimation()
    {
        /*        animator.Play("Windup");
                yield return new WaitForSeconds(1); 
                animator.Play("Shake");
                yield return new WaitForSeconds(1);
                animator.Play("Drop");
                yield return new WaitForSeconds(1);
                animator.Play("Reset");*/
        animator.SetTrigger("Peel Attack");
        yield return new WaitForSeconds(0.1f);
    }

    void PeelSlamCooldown()
    {
        // add animation here
    }

    private GameObject SpawnBoomerang(Vector3 position, int radiusAdd)
    {
        GameObject boomerang = Instantiate(orangeSliceBoomerangs, position, Quaternion.identity);
        CircularMovement circularMovement = boomerang.GetComponent<CircularMovement>();
        circularMovement.target = origin.transform;
        circularMovement.direction = position.x > transform.position.x ? 1 : -1;
        circularMovement.angle = (position.x < transform.position.x ? 180f + transform.eulerAngles.y : 0f + transform.eulerAngles.y) * Mathf.Deg2Rad;
        circularMovement.radius += (radiusAdd * 7);
        return boomerang;
    }

    void Cooldown()
    {
        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0)
        {
            currMove++;
            state = BossStates.IDLE;
        }
    }

    IEnumerator DestroyBoomerangs(GameObject x, GameObject y)
    {
        yield return new WaitForSeconds(boomerangCooldown);
        Destroy(x);
        Destroy(y);
        boomerangSpinning = false;
    }

    public void Damage(int dmg)
    {
        health -= dmg;
        healthUI.fillAmount = health / (1.0f * maxHealth);
        if (health == 0)
        {
            print("BOSS DEFEATED");
            // TODO: GO TO SOME SORT OF WIN SCREEN. FOR NOW GO TO MAIN MENU
            SceneManager.LoadScene(0);
        }
    }
}

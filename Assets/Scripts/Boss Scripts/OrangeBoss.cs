using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrangeBoss : MonoBehaviour
{
    [Header("Atacks")]
    public GameObject orangeSliceBoomerangs;
    public GameObject minions;
    private readonly int moves = 3;
    private int currMove;
    public bool indicating = false;
    public bool boomerangSpinning = false;

    [Header("Cooldown")]
    public float boomerangCooldown;
    public float spawnCooldown;
    public float peelCooldown;
    public float peelAnimationTime;
    private float cooldownTimer;

    public Animator modelAnimator;
    public Animator healthAnimator;

    public GameObject[] spawnPoints;
    public GameObject origin;
    public GameObject player;
    public List<GameObject> boomerangObjects;
    public List<GameObject> weakSpots;

    public BossStates state;

    [Header("Damage")]
    public int maxHealth;
    private int health;
    public Image healthUI;
    public Material normalColor;
    public Material hurtColor;
    public GameObject[] partsOfModel;

    public enum BossStates
    {
        IDLE, BOOMERANG, PEEL, SPAWN, COOLDOWN, NONE
    };

    private void Start()
    {
        state = BossStates.NONE;
        StartCoroutine(BoomerangStartUpHelper()); // Give a pause before boss battle starts

        //state = BossStates.PEEL;

        health = maxHealth;
        currMove = 0;

        player = GameObject.FindWithTag("Player");
        indicating = false;
        boomerangSpinning = false;

    }

    private void Update()
    {
        if (player != null && !indicating)
        {
            transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z));
        }

        switch (state)
        {
            case BossStates.IDLE:
                if (currMove % moves == 0)
                {
                    state = BossStates.BOOMERANG;
                }
                else if (currMove % moves == 1)
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
    }

    void SpawnBoomerangs()
    {
        cooldownTimer = 3.5f + boomerangCooldown;
        state = BossStates.COOLDOWN;
        StartCoroutine(BoomerangStartup());
    }

    IEnumerator SpinningBoomerangs()
    {
        while (boomerangSpinning)
        {
            yield return new WaitForEndOfFrame();
            SoundManager.Instance().PlaySFX("OrangeBossBoomerangs");
        }
    }
    IEnumerator BoomerangStartUpHelper()
    {
        yield return new WaitForSeconds(2f);
        state = BossStates.IDLE;
    }

    IEnumerator BoomerangStartup()
    {
        modelAnimator.SetTrigger("Boomerang Attack");
        yield return new WaitForSeconds(2.5f);
        for (int i = 0; i < 5; i++)
        {
            GameObject boomerangRight = SpawnBoomerang(spawnPoints[0].transform.position + spawnPoints[0].transform.right, i);
            GameObject boomerangLeft = SpawnBoomerang(spawnPoints[1].transform.position - spawnPoints[1].transform.right, i);
            boomerangObjects.Add(boomerangLeft);
            boomerangObjects.Add(boomerangRight);
            StartCoroutine(DestroyBoomerangs(boomerangRight, boomerangLeft));
        }
        indicating = true;
        yield return new WaitForSeconds(1.5f);
        indicating = false;
        foreach (GameObject b in boomerangObjects)
        {
            b.GetComponent<CircularMovement>().SetCollider(true);
        }
        boomerangObjects.Clear();
        boomerangSpinning = true;
        StartCoroutine(SpinningBoomerangs());
    }

    void SpawnEnemies()
    {
        // Add animation here

        SoundManager.Instance().PlaySFX("OrangeBossSummon");
        modelAnimator.SetTrigger("Spawn Orange");
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
        cooldownTimer = spawnCooldown;
        state = BossStates.COOLDOWN;
    }

    void PeelSlam()
    {
        // add animation here
        indicating = true;
        modelAnimator.SetTrigger("Peel Attack");
        StartCoroutine(PeelSlamCooldown());
        cooldownTimer = peelAnimationTime + peelCooldown;
        state = BossStates.COOLDOWN;
    }

    IEnumerator PeelSlamCooldown()
    {
        yield return new WaitForSeconds(peelAnimationTime + peelCooldown);
        modelAnimator.SetTrigger("Peel Reset");
        indicating = false;
    }

    private GameObject SpawnBoomerang(Vector3 position, int radiusAdd)
    {
        GameObject boomerang = Instantiate(orangeSliceBoomerangs, position, Quaternion.identity);
        CircularMovement circularMovement = boomerang.GetComponent<CircularMovement>();
        circularMovement.target = origin.transform;
        circularMovement.direction = position.x > transform.position.x ? 1 : -1;
        circularMovement.angle = (position.x < transform.position.x ? 180f + transform.eulerAngles.y : 0f + transform.eulerAngles.y) * Mathf.Deg2Rad;
        circularMovement.radius += (radiusAdd * 7);
        circularMovement.SetCollider(false);
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
        healthAnimator.SetTrigger("DamageWeak"); // in case we want to make weak spots have diff anim
        healthUI.fillAmount = health / (1.0f * maxHealth);
        StartCoroutine(FlashDamage());

        if (health == 0)
        {
            print("BOSS DEFEATED");
            // TODO: GO TO SOME SORT OF WIN SCREEN. FOR NOW GO TO MAIN MENU
            LevelSwitch.ChangeScene("Menu");
        }
    }

    IEnumerator FlashDamage()
    {
        for (int i = 0; i < partsOfModel.Length; i++)
        {
            partsOfModel[i].GetComponent<Renderer>().material = hurtColor;
        }
        yield return new WaitForSeconds(0.3f);
        for (int i = 0; i < partsOfModel.Length; i++)
        {
            partsOfModel[i].GetComponent<Renderer>().material = normalColor;
        }
        yield return new WaitForSeconds(0.3f);
    }

    public void ShowWeakSpot(int weakSpotIndex)
    {
        weakSpots[weakSpotIndex].SetActive(true);
    }

    public void HideWeakSpots()
    {
        foreach(GameObject o in weakSpots)
        {
            o.SetActive(false);
        }
    }
}

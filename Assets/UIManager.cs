using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Sprite[] healthSprites;
    public GameObject healthSprite;

    public void ChangeHealthImage(int health)
    {
        healthSprite.GetComponent<Image>().sprite = healthSprites[health];
    } 
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Sprite[] healthSprites;
    public GameObject healthSprite;
    public GameObject reticuleSprite;

    public void ChangeHealthImage(int health)
    {
        healthSprite.GetComponent<Image>().sprite = healthSprites[health];
    } 

    public void ReticleOverLassoable()
    {
        reticuleSprite.GetComponent<Image>().color = Color.red;
    }

    public void ReticleReset()
    {
        reticuleSprite.GetComponent<Image>().color = Color.white;
    }
}

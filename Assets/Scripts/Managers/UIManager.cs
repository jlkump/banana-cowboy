using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Sprite[] healthSprites;
    public GameObject healthSprite;
    public Sprite[] reticuleSprites;
    public GameObject reticuleSprite;
    public GameObject throwBarSpriteRoot;
    public Image throwBar;
    public Image throwBarIndicator;
    public Transform throwBarIndicatorStartPos;
    public Transform throwBarIndicatorEndPos;
    public Transform throwWeakStartPos;
    public Transform throwMediumStartPos;
    public Transform throwStrongStartPos;

    public void ChangeHealthImage(int health)
    {
        if (health >= 0 && health < 5)
        {
            healthSprite.GetComponent<Image>().sprite = healthSprites[health];
        }
    } 

    public void ReticleOverLassoable()
    {
        reticuleSprite.GetComponent<Image>().sprite = reticuleSprites[1];
    }

    public void ReticleReset()
    {
        reticuleSprite.GetComponent<Image>().sprite = reticuleSprites[0];
    }

    public void ShowThrowBar()
    {
        throwBarSpriteRoot.SetActive(true);
    }

    public void HideThrowBar()
    {
        throwBarSpriteRoot.SetActive(false);
    }

    /**
     * relativePos is on the range [-1, 1] with 0 being the center of the bar.
     */
    public void SetThrowIndicatorPos(float relativePos)
    {
        float halfWidth = (throwBarIndicatorEndPos.position.x - throwBarIndicatorStartPos.position.x) / 2;
        throwBarIndicator.rectTransform.position = new Vector3(
            throwBarIndicatorStartPos.position.x + halfWidth + relativePos * halfWidth, 
            throwBarIndicatorStartPos.position.y, 
            throwBarIndicatorStartPos.position.z
        );
    }

    public PlayerController.ThrowStrength GetThrowIndicatorStrength()
    {
        Vector3 throwBarPos = throwBarIndicator.rectTransform.position;
        Vector3 center = throwBar.rectTransform.position;
        if ((throwBarPos.x < center.x && throwBarPos.x > throwStrongStartPos.position.x) || 
            (throwBarPos.x > center.x && throwBarPos.x < ((center.x - throwStrongStartPos.position.x)) + center.x))
        {
            return PlayerController.ThrowStrength.STRONG;
        }
        if ((throwBarPos.x < center.x && throwBarPos.x > throwMediumStartPos.position.x) ||
            (throwBarPos.x > center.x && throwBarPos.x < ((center.x - throwMediumStartPos.position.x)) + center.x))
        {
            return PlayerController.ThrowStrength.MEDIUM;
        }
        return PlayerController.ThrowStrength.WEAK;
    }
}

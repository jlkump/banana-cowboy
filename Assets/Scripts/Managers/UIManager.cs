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

    [SerializeField]
    RectTransform throwBarContainer, throwLowPower, throwMedPower, throwHighPow, throwBarIndicator;

    Vector3 indicatorStartingPos;

    private void Awake()
    {
        if (throwBarIndicator != null)
        {
            indicatorStartingPos = throwBarIndicator.localPosition;
        }
    }

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
        if (throwBarIndicator == null) { return; }
        float width = (throwLowPower.rect.width) * throwLowPower.localScale.x;
        float halfWidth = width / 2;
        throwBarIndicator.localPosition = new Vector3(
            indicatorStartingPos.x + relativePos * halfWidth,
            indicatorStartingPos.y, 
            indicatorStartingPos.z
        );
    }

    public PlayerController.ThrowStrength GetThrowIndicatorStrength()
    {
        float lowPowerWidth = (throwLowPower.rect.width / 2f) * throwLowPower.localScale.x;
        float medPowerWidth = (throwMedPower.rect.width / 2f) * throwMedPower.localScale.x;
        float highPowerWidth = (throwHighPow.rect.width / 2f) * throwHighPow.localScale.x;

        if ((throwBarIndicator.localPosition.x > 0 && throwBarIndicator.localPosition.x < (highPowerWidth)) || 
            (throwBarIndicator.localPosition.x < 0 && throwBarIndicator.localPosition.x > -(highPowerWidth)))
        {
            return PlayerController.ThrowStrength.STRONG;
        }
        if (throwBarIndicator.localPosition.x > 0 && throwBarIndicator.localPosition.x < medPowerWidth ||
            (throwBarIndicator.localPosition.x < 0 && throwBarIndicator.localPosition.x > -medPowerWidth))
        {
            return PlayerController.ThrowStrength.MEDIUM;
        }
        return PlayerController.ThrowStrength.WEAK;
    }
}

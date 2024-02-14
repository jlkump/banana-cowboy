using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrangeBossAnimationManager : MonoBehaviour
{
    public OrangeBoss instance;
    public void ShowWeakSpot(int weakSpotIndex)
    {
        instance.ShowWeakSpot(weakSpotIndex);
    }
}

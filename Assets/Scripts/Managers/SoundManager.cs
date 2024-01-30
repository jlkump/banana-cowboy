using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This SoundManager class will be used in casses where sfx
 * are played to the player regardless of distance to an object.
 * For example, it will play ???? (We might just want everything to handle its own sound)
 */
public class SoundManager : MonoBehaviour
{

    private static SoundManager s_Instance = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static float S_GetSFXVolume()
    {
        return 1.0f;
    }
}

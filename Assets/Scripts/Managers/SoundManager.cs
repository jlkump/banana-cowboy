using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/**
 * This SoundManager class will be used in casses where sfx
 * are played to the player regardless of distance to an object.
 * For example, it will play ???? (We might just want everything to handle its own sound)
 */
public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    private static SoundManager s_Instance = null;

    // This should be on a range [0, 1] (representing the 0% to 100%)
    public float SFXVolume { get; set; } = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        s_Instance = this;
        foreach (Sound s in sounds) { 
            s.src = gameObject.AddComponent<AudioSource>();
            s.src.clip = s.audioClip;
            s.src.volume = s.volume;
            s.src.pitch = s.pitch;
        }
    }

    public void Play(string name)
    {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s.type == Sound.Type.SFX)
        {
            s.src.volume = s.volume * SFXVolume;
        }
        s.src.Play();
        if (!s.src.isPlaying)
        {

        }
    }

    public Sound GetSound(string name)
    {
        return Array.Find(sounds, sound => sound.name == name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static SoundManager S_Instance()
    { 
        return s_Instance; 
    }
}
[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip audioClip;
    [Range(0, 1)]
    public float volume = 1.0f;
    [Range(0.1f, 3)]
    public float pitch = 1.0f;
    public enum Type
    {
        SFX,
        AMBIENT,
        MUSIC,
    };

    public Type type;

    [HideInInspector]
    public AudioSource src;
}
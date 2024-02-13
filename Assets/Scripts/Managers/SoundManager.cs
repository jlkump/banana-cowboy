using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

/**
 * This SoundManager class will be used in casses where sfx
 * are played to the player regardless of distance to an object.
 * For example, it will play ???? (We might just want everything to handle its own sound)
 */
public class SoundManager : MonoBehaviour
{
    public Sound[] sfxs;
    public Sound[] music;

    private static SoundManager s_Instance = null;
    private List<Sound> _loopedSounds = new List<Sound>();

    // This should be on a range [0, 1] (representing the 0% to 100%)
    public float SFXVolume { get; set; } = 1.0f;
    public float MusicVolume { get; set; } = 1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        s_Instance = this;
        foreach (Sound s in sfxs) { 
            s.src = gameObject.AddComponent<AudioSource>();
            s.src.clip = s.audioClip;
            s.src.volume = s.volume;
            s.src.pitch = s.pitch;
            s.type = Sound.Type.SFX;
        }

        foreach (Sound s in music)
        {
            s.src = gameObject.AddComponent<AudioSource>();
            s.src.clip = s.audioClip;
            s.src.volume = s.volume;
            s.src.pitch = s.pitch;
            s.type = Sound.Type.MUSIC;
        }
    }

    void Update()
    {
        foreach (Sound s in _loopedSounds.ToArray())
        {
            if (!s.src.isPlaying)
            {
                s.src.volume = s.volume * SFXVolume;
                s.src.pitch = s.pitch + UnityEngine.Random.Range(0.0f, s.pitchVariance);
                s.src.Play();
            }
        }
    }

    public void PlaySFX(string name)
    {
        Sound s = Array.Find(sfxs, sound => sound.name == name);
        if (s == null) { return; }

        s.src.volume = s.volume * SFXVolume;
        s.src.pitch = s.pitch + UnityEngine.Random.Range(0.0f, s.pitchVariance);

        if (!s.src.isPlaying && !PauseManager.pauseActive)
        {
            if (s.loop)
            {
                if (Array.Find(_loopedSounds.ToArray(), sound => sound.name == name) == null)
                {
                    _loopedSounds.Add(s);
                }
            } 
            else
            {
                s.src.Play();
            }
        }
    }

    public Sound GetSFX(string name)
    {
        return Array.Find(sfxs, sound => sound.name == name);
    }

    public void StopSFX(string name)
    {
        Sound s = Array.Find(sfxs, sound => sound.name == name);
        if (s == null) { return; }
        if (s.loop && Array.Find(_loopedSounds.ToArray(), sound => sound.name == name) != null)
        {
            _loopedSounds.Remove(s);
        }
        else if (s.src.isPlaying)
        {
            s.src.Stop();
        }
    }

    public void PlayMusic(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null) { return; }
        s.src.volume = s.volume * SFXVolume;

        if (!s.src.isPlaying)
        {
            s.src.Play();
        }
    }

    public void PauseMusic(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null) { return; }

        if (s.src.isPlaying)
        {
            s.src.Pause();
        }
    }

    public void StopMusic(string name)
    {
        Sound s = Array.Find(music, sound => sound.name == name);
        if (s == null) { return; }

        if (s.src.isPlaying)
        {
            s.src.Stop();
        }
    }

    public Sound GetMusic(string name)
    {
        return Array.Find(music, sound => sound.name == name);
    }

    public static SoundManager Instance()
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
    [Range(0.0f, 3)]
    public float pitchVariance = 0.0f;
    public bool loop = false;
    public enum Type
    {
        SFX,
        AMBIENT,
        MUSIC,
    };

    [HideInInspector]
    public Type type;

    [HideInInspector]
    public AudioSource src;
}
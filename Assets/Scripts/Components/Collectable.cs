using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Collectable : MonoBehaviour
{
    public AudioClip collectSFX;
    public AudioSource source;

    private void Start()
    {
        source.clip = collectSFX;
        source.volume = SoundManager.S_GetSFXVolume();
        source.pitch = 1.0f;
        source.loop = false;
    }

    private void OnTriggerEnter(Collider item)
    {
        if (item.CompareTag("Player"))
        {
            source.Play();
            // Perform after delay and just destroy rendered instead?
            Invoke("DestroyMe", collectSFX.length);
            Destroy(GetComponent<Renderer>());
        }
    }

    void DestroyMe()
    {
        Destroy(gameObject);
    }
}

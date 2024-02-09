using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Collectable : MonoBehaviour
{

    private void OnTriggerEnter(Collider item)
    {
        if (item.CompareTag("Player"))
        {
            SoundManager.S_Instance().Play("CollectStarSparkle");
            Destroy(gameObject);
        }
    }
}

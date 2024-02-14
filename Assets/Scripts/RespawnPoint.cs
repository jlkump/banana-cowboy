using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [field: SerializeField] public int checkpointNum { get; private set; }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Set checkpoint " + checkpointNum + ".");
            // set game managers respawn coords to set coordinates
            LevelData.setCheckpoint(checkpointNum);
        }
    }
}

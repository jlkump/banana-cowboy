using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class LevelSwitch : MonoBehaviour
{
    public string menuScene;
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.tag == "Player" && !menuScene.IsUnityNull())
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
            LevelData.resetLevelData();
            SoundManager.Instance().StopAllSFX();
            SceneManager.LoadScene(menuScene); 
            if(menuScene == "Orange Boss Scene")
            {
                if (SoundManager.Instance() != null)
                {
                    SoundManager.Instance().StopMusic("Orange Planet");
                    SoundManager.Instance().PlayMusic("Orange Boss");
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class TutorialEnd : MonoBehaviour
{
    // TODO: Change name so that it can work for other levels instead of only tutorial
    public string menuScene;
    private void OnTriggerEnter(Collider other)
    {
        if (other != null && other.gameObject.tag == "Player" && !menuScene.IsUnityNull())
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.visible = true;
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

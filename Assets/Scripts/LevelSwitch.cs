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
            ChangeScene(menuScene);
            /*UnityEngine.Cursor.lockState = CursorLockMode.None;
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
            }*/
        }
    }

    public static void ChangeScene(string scene)
    {
        if (scene == "Menu")
        {
            if (SoundManager.Instance() != null)
            {
                SoundManager.Instance().StopAllMusic();
                SoundManager.Instance().PlayMusic("Main Menu");
            }
        }
        else if (scene == "Orange Boss Scene")
        {
            if (SoundManager.Instance() != null)
            {
                SoundManager.Instance().StopMusic("Orange Planet");
                SoundManager.Instance().PlayMusic("Orange Boss");
            }
        }
        PlayerCameraController.ShowCursor();
        LevelData.resetLevelData();
        SoundManager.Instance().StopAllSFX();
        SceneManager.LoadScene(scene);
    }
}

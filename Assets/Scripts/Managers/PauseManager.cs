using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingScreen;
    public static bool pauseActive;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu.activeSelf)
            {
                pauseMenu.SetActive(false);
                settingScreen.SetActive(false);
                pauseActive = true;
                Time.timeScale = 1.0f;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
            }
            else
            {
                pauseMenu.SetActive(true);
                pauseActive = false;
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
        }
    }

    public void ChangeScreen(Button button)
    {
        switch (button.name)
        {
            case "Continue":
                pauseMenu.SetActive(false);
                Time.timeScale = 1.0f;
                UnityEngine.Cursor.lockState = CursorLockMode.Locked;
                UnityEngine.Cursor.visible = false;
                break;
            case "Checkpoint Restart":
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case "Settings":
                settingScreen.SetActive(true);
                break;
            case "Back":
                settingScreen.SetActive(false);
                break;
            case "Restart":
                Time.timeScale = 1.0f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                break;
            case "Quit":
                Time.timeScale = 1.0f;
                break;
        }
    }

}

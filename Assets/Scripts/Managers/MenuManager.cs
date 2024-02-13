using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject settings;
    public GameObject help;
    public GameObject credits;

    public void ChangeScreen(Button button)
    {
        switch (button.name)
        {
            case "Play":
                // Go to level selection screen
                SceneManager.LoadScene(1);
                break;
            case "Settings":
                settings.SetActive(true);
                break;
            case "Help":
                help.SetActive(true);
                break;
            case "Credits":
                credits.SetActive(true);
                break;
            case "Quit":
                Application.Quit();
                break;
            case "Back":
                settings.SetActive(false);
                help.SetActive(false);
                credits.SetActive(false);
                break;
        }
    }
}

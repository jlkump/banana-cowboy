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

    public Slider musicSlider = null;
    public Slider sfxSlider = null;
    private void Start()
    {
        if (musicSlider != null && SoundManager.Instance() != null)
        {
            musicSlider.value = SoundManager.Instance().MusicVolume;
            musicSlider.onValueChanged.AddListener(delegate { MusicValueChanged(); });
        }

        if (sfxSlider != null && SoundManager.Instance() != null)
        {
            sfxSlider.value = SoundManager.Instance().SFXVolume;
            sfxSlider.onValueChanged.AddListener(delegate { SFXValueChanged(); });
        }

        if (SoundManager.Instance() != null)
        {
            SoundManager.Instance().PlayMusic("Main Menu");
        }
    }

    public void MusicValueChanged()
    {
        SoundManager.Instance().MusicVolume = musicSlider.value;
    }

    public void SFXValueChanged()
    {
        SoundManager.Instance().SFXVolume = sfxSlider.value;
    }


    public void ChangeScreen(Button button)
    {
        switch (button.name)
        {
            case "Play":
                // Go to Backstory Cutscene
                SceneManager.LoadScene("Cutscene1");

                // Go to level selection screen
                // if (SoundManager.Instance() != null)
                // {
                    // SoundManager.Instance().StopMusic("Main Menu");
                    // SoundManager.Instance().PlayMusic("Orange Planet");
                // }
                // SceneManager.LoadScene("Orange Level");
                break;
            case "Settings":
                settings.SetActive(true);
                break;
            case "Tutorial":
                SceneManager.LoadScene("Tutorial Level");
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ComicCutsceneManager : MonoBehaviour
{
    public float fadeTime = 0.5f;
    public float timeBetweenBoxes = 1.5f;
    public string nextScene;
    public GameObject skipButton;

    public List<GameObject> panelGroups = new List<GameObject>();
    private int currPanel = 0;
    private bool endOfCutscene;

    [System.Serializable]
    public class Boxes
    {
        public List<GameObject> boxes;
    }
    public List<Boxes> panels = new List<Boxes>();

    public void Start()
    { 
        endOfCutscene = false;

        foreach (GameObject panel in panelGroups)
        {
            panel.SetActive(false);
        }

        // fade in skip button
        skipButton.GetComponent<Image>().color -= new Color(0, 0, 0, 1);
        StartCoroutine("SkipButtonAnimation");

        StartCoroutine("PanelAnimation");
    }

    IEnumerator SkipButtonAnimation()
    {
        yield return new WaitForSeconds(0.1f);
        Color endColor = skipButton.GetComponent<Image>().color + new Color(0, 0, 0, 1);
        skipButton.GetComponent<Image>().DOColor(endColor, 1f).SetEase(Ease.OutExpo);
    }


    IEnumerator PanelAnimation()
    {
        panelGroups[currPanel].SetActive(true);

        // initially set all panels to have 0 alpha
        foreach (GameObject box in panels[currPanel].boxes)
        {
            // box.transform.localScale = Vector3.zero;
            box.GetComponent<Image>().color -= new Color(0, 0, 0, 1);
        }

        // wait before starting
        yield return new WaitForSeconds(1f);

        // scaling animation
        foreach (GameObject box in panels[currPanel].boxes)
        {
            Color endColor = box.GetComponent<Image>().color + new Color(0, 0, 0, 1);

            // box.transform.DOScale(1f, fadeTime).SetEase(Ease.OutSine);
            box.GetComponent<Image>().DOColor(endColor, fadeTime).SetEase(Ease.OutExpo);
            yield return new WaitForSeconds(timeBetweenBoxes);
        }
    }

    public void FadeOut(int currPanel)
    {
        panelGroups[currPanel].GetComponent<CanvasGroup>().alpha = 1f;
        panelGroups[currPanel].GetComponent<CanvasGroup>().DOFade(0f, fadeTime);
    }
    
    // continue button is pressed, should fade out the entire panel and start the next one 
    public void Continue()
    {
        if (!endOfCutscene)
        {
            FadeOut(currPanel);
            currPanel += 1;
            if (currPanel < panels.Count)
            {   
                StartCoroutine("PanelAnimation");
            }
            else
            {
                endOfCutscene = true;
                StartCoroutine("ChangeScene");
            }
        }
    }

    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1.2f);
        switch (nextScene)
        {
            case "Tutorial Level":
                break;
            case "Orange Level":
                if (SoundManager.Instance() != null)
                {
                    SoundManager.Instance().StopMusic("Main Menu");
                    SoundManager.Instance().PlayMusic("Orange Planet");
                }
                break;

        }
        SceneManager.LoadScene(nextScene);
    }

    public void SkipCutscene()
    {
        endOfCutscene = true;
        StopAllCoroutines();
        FadeOut(currPanel);
        StartCoroutine("ChangeScene");
    }
}

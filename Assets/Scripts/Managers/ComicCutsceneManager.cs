using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class ComicCutsceneManager : MonoBehaviour
{
    // cutscene = sequence of panels
    // panel = full spread
    // box = one part of panel 

    public float fadeTime = 0.2f;
    public float timeBetweenBoxes = 3f;
    public string nextScene;
    public List<GameObject> panelGroups = new List<GameObject>();
    // public List<List<GameObject>> panels = new List<List<GameObject>>();
    // public List<GameObject> boxes1 = new List<GameObject>();
    // public List<GameObject> boxes2 = new List<GameObject>();

    

    public int test = 0;
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

        StartCoroutine("PanelAnimation");
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
        SceneManager.LoadScene(nextScene);
    }
}

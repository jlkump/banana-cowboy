using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

/**
 * Cutscene managers are per-scene. If any cutscene is
 * called when it does not exist, it will not be played
 */
public class CutsceneManager : MonoBehaviour
{
    private static CutsceneManager s_instance = null;

    [Tooltip("The input for skipping / going to the next scene")]
    public KeyCode cutsceneInput = KeyCode.Mouse0;
    [Tooltip("The name of the cutscene to play when the scene starts")]
    public string cutsceneOnStart = string.Empty;
    [Tooltip("The list of cutscenes that can be played by this manager")]
    public Cutscene[] cutscenes;

    private Cutscene activeCutscene = null;

    private PlayerController _player = null;

    void Start()
    {
        s_instance = this;
        _player = FindAnyObjectByType<PlayerController>();
        s_instance.PlayStartCutscene();
    }

    private void Update()
    {
        if (activeCutscene != null && !activeCutscene.cutscene.IsCutsceneComplete())
        {
            //if (Input.GetKeyDown(cutsceneInput))
            //{
            //    activeCutscene.cutscene.SkipActiveScene();
            //}
        }
    }

    public void PlayStartCutscene()
    {
        if (cutsceneOnStart != string.Empty)
        {
            PlayCutsceneByName(cutsceneOnStart);
        }
    }

    public void PlayCutsceneByName(string name)
    {
        Cutscene cs = Array.Find(cutscenes, cs => cs.name == name);
        if (cs == null) { return; }
        if (activeCutscene != null) { 
            activeCutscene.cutscene.Stop();
            activeCutscene.cutscene.OnCutsceneComplete -= CutsceneFinished;
        }
        activeCutscene = cs;
        activeCutscene.cutscene.OnCutsceneComplete += CutsceneFinished;
        activeCutscene.cutscene.Play();
        _player.DisableForCutscene();
    }

    void CutsceneFinished(CutsceneObject completedScene)
    {
        _player.EnableAfterCutscene();
    }

    public static CutsceneManager Instance()
    {
        return s_instance;
    }
}

[System.Serializable]
public class Cutscene
{
    public string name;
    public CutsceneObject cutscene;
}

using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class CutsceneObject : MonoBehaviour
{
    [Tooltip("Plays scenes in the order they are listed in the array")]
    public Scene[] scenes;

    private int activeScene = 0;
    private bool _cutsceneComplete = false;
    private bool _skip = false;
    private bool _stopCutscene = false;

    public delegate void CutsceneCompleteCallback(CutsceneObject completedScene);
    public event CutsceneCompleteCallback OnCutsceneComplete;

    public void Play()
    {
        _cutsceneComplete = false;
        _stopCutscene = false;
        StopAllCoroutines();
        StartCoroutine(PlayScenes());
    }
    public bool IsCutsceneComplete()
    {
        return _cutsceneComplete;
    }

    IEnumerator PlayScenes()
    {
        while (activeScene < scenes.Length && !_stopCutscene)
        {
            _skip = false;
            PlayScene(scenes[activeScene]);
            yield return WaitForSceneCompletion(scenes[activeScene]);
            CleanupScene(scenes[activeScene]);
            activeScene++;
        }
        _cutsceneComplete = true;
        DialogueManager.Instance().HideActiveDialog();
        OnCutsceneComplete?.Invoke(this); // This is apparently a null check on OnCutsceneComplete, so things are good
    }

    void PlayScene(Scene scene)
    {
        if (scene.isDialogScene)
        {
            DialogueManager.Instance().DisplayDialog(scene.dialog);
        }

        if (scene.isTimelineScene)
        {
            scene.cinemachineCamController.gameObject.SetActive(true);
            scene.timelinePlayable.extrapolationMode = DirectorWrapMode.Hold;
            scene.timelinePlayable.Play();
        }

        if (scene.cameraShake)
        {
            // TODO:
        }
    }

    void CleanupScene(Scene scene)
    {
        if (scene.isDialogScene)
        {
            DialogueManager.Instance().HideDialog(scene.dialog);
        }
        if (scene.isTimelineScene)
        {
            scene.cinemachineCamController.gameObject.SetActive(false);
            scene.timelinePlayable.Stop();
        }
    }

    public void SkipActiveScene()
    {
        _skip = true;
    }

    public void Stop()
    {
        _stopCutscene = true;
        SkipActiveScene();
    }

    IEnumerator WaitForSceneCompletion(Scene scene)
    {
        switch (scene.completionType)
        {
            case Scene.CompletionType.ON_INPUT:
                yield return WaitForInput();
                break;
            case Scene.CompletionType.ON_DIALOG_COMPLETE:
                yield return WaitForDialog(scene.dialog);
                break;
            case Scene.CompletionType.ON_PLAYABLE_COMPLETE:
                yield return WaitForPlayable(scene.timelinePlayable);
                break;
        }
        if (!_skip)
        {
            yield return new WaitForSeconds(scene.completionDelay);
        }
    }

    IEnumerator WaitForInput()
    {
        bool done = false;
        while (!done && !_skip)
        {
            if (Input.GetKeyDown(CutsceneManager.Instance().cutsceneInput))
            {
                done = true;
            }
            yield return null;
        }
    }

    IEnumerator WaitForDialog(Dialog dialog)
    {
        if (dialog != null)
        {            
            bool done = false;
            while (!done && !_skip)
            {
                if (dialog.dialogFullyDisplayed)
                {
                    done = true;
                }
                yield return null;
            }
        }
    }

    IEnumerator WaitForPlayable(PlayableDirector playable)
    {
        if (playable != null)
        {
            bool done = false;
            while (!done) // && !_skip) for now, no skipping the camera / playables. Too hard to debug
            {
                if (playable.time >= playable.duration)
                {
                    done = true;
                }
                yield return null;
            }
            playable.time = playable.duration;
        }
    }
}

[System.Serializable]
public class Scene
{
    public bool isDialogScene = false;
    [Tooltip("Only required if isDialogScene is true")]
    public Dialog dialog;

    public bool isTimelineScene = false;
    [Tooltip("Only required if isTimelineScene is true")]
    public PlayableDirector timelinePlayable = null;
    [Tooltip("Start this disabled. Will enable on cutscene play")]
    public CinemachineVirtualCamera cinemachineCamController = null;

    [Tooltip("Done only at the start of the scene")]
    public bool cameraShake = false;

    public enum CompletionType
    {
        ON_INPUT,
        ON_DIALOG_COMPLETE,
        ON_PLAYABLE_COMPLETE
    }
    public CompletionType completionType = CompletionType.ON_INPUT;
    [Tooltip("This is the time between the completion of the scene and the start of the next scene")]
    public float completionDelay = 1.5f; // Adds time between the completion of a scene and the start of the next scene
}

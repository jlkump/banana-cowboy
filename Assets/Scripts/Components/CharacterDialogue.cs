using UnityEngine;


public class CharacterDialogue : MonoBehaviour
{
    public Dialog dialog;

    private void OnTriggerEnter(Collider collision)
    {
        if (dialog.speakerDialog != string.Empty) { 
            if (collision.CompareTag("Player"))
            {
                DialogueManager.Instance().DisplayDialog(dialog);
            }
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (dialog.speakerDialog != string.Empty)
        {
            if (collision.CompareTag("Player"))
            {
                DialogueManager.Instance().HideDialog(dialog);
            }
        }
    }
}
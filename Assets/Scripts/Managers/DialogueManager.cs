using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static CharacterDialogue;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialogueHolder;
    public TMP_Text textDisplay;
    public TMP_Text nameOfCharacter;
    public Image charPortrait;

    // TODO might create a dictionary to get the portrait corresponding to the type/ name.
    public List<GameObject> portraits;

    private readonly float _typingSpeed = 0.02f;
    private static string s_text;

    private static DialogueManager s_instance;
    private Coroutine typingCoroutine;

    private void Start()
    {
        s_instance = this;
/*        dialogueHolder = transform.GetChild(0).gameObject;
        textDisplay = dialogueHolder.transform.GetChild(1).GetComponent<TMP_Text>();*/
    }
    IEnumerator Type(string sentence)
    {
        textDisplay.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            textDisplay.text += letter;
            yield return new WaitForSeconds(_typingSpeed);
        }
    }

    public static void StartText(string sentence, string name, Enum type)
    {
        s_text = sentence;
        s_instance.nameOfCharacter.text = name;
        // TODO Depending on what the enemy is, put the image here. For now change color and use enum
        Color colorChar;
        switch (type)
        {
            case TypeOfCharacter.Strawberry:
                colorChar = Color.red;
                break;
            case TypeOfCharacter.Blueberry:
                colorChar = Color.blue;
                break;
            case TypeOfCharacter.Orange:
                colorChar = Color.yellow;
                break;
            default: 
                colorChar = Color.white;
                break;
        }
        s_instance.charPortrait.color = colorChar;

        s_instance.dialogueHolder.SetActive(true);
        s_instance.typingCoroutine = s_instance.StartCoroutine(s_instance.Type(sentence));
    }

    public static void StopText()
    {
        if (s_instance.typingCoroutine != null)
        {
            s_instance.StopCoroutine(s_instance.typingCoroutine);
        }
        s_instance.dialogueHolder.SetActive(false);
    }
}
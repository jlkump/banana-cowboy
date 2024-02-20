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

    public GameObject portraitHolder = null;
    public GameObject nameHolder = null;

    // TODO might create a dictionary to get the portrait corresponding to the type/ name.
    public List<GameObject> portraits;

    private readonly float _typingSpeed = 0.02f;
    private static string s_text;

    private static DialogueManager s_instance;
    private Coroutine typingCoroutine;



    private void Start()
    {
        s_instance = this;
        s_text = "";
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
        //if (s_text.CompareTo(sentence) == 0) { return; }
        s_text = sentence;
        s_instance.nameOfCharacter.text = name;
        // TODO Depending on what the enemy is, put the image here. For now change color and use enum
        Color colorChar;
        Color colorBox;
        bool showPortrait = true;
        bool showName = true;
        switch (type)
        {
            case TypeOfCharacter.Strawberry:
                colorChar = Color.red;
                colorBox = s_instance.ConvertToColor(255, 146, 146);
                break;
            case TypeOfCharacter.Blueberry:
                colorChar = Color.blue;
                colorBox = s_instance.ConvertToColor(106, 201, 255);
                break;
            case TypeOfCharacter.Orange:
                colorChar = s_instance.ConvertToColor(255, 93, 0);
                colorBox = s_instance.ConvertToColor(168, 93, 50);
                break;
            case TypeOfCharacter.Banana:
                colorChar = Color.yellow;
                colorBox = s_instance.ConvertToColor(211, 166, 0);
                break;
            default: 
                colorChar = Color.white;
                colorBox = Color.white;
                showPortrait = false;
                showName = false;
                break;
        }
        if (s_instance.portraitHolder != null)
        {
            s_instance.portraitHolder.SetActive(showPortrait);
        }
        if (s_instance.nameHolder != null)
        {
            s_instance.nameHolder.SetActive(showName);
        }

        s_instance.charPortrait.color = colorChar;
        s_instance.dialogueHolder.GetComponent<Image>().color = colorBox;

        s_instance.dialogueHolder.SetActive(true);
        s_instance.typingCoroutine = s_instance.StartCoroutine(s_instance.Type(sentence));
    }

    private Color ConvertToColor(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
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
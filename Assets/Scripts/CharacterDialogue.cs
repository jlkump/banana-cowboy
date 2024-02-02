using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;


public class CharacterDialogue : MonoBehaviour
{
    public string dialogueText;
    public bool boss;
    public string nameOfCharacter;
    public TypeOfCharacter typeOfCharacter;
    public enum TypeOfCharacter
    {
        None,Strawberry,Blueberry,Orange
    };

    private void OnTriggerEnter(Collider collision)
    {
        print("here");
        if (collision.CompareTag("Player"))
        {
            DialogueManager.StartText(dialogueText, nameOfCharacter, typeOfCharacter);
        }
    }

    private void OnTriggerExit(Collider collision)
    {
        if (collision.CompareTag("Player"))
        {
            DialogueManager.StopText();
        }
    }
}
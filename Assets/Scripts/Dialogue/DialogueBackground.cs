using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(DialogueController))]
public class DialogueBackground : MonoBehaviour
{
    public Image backgroundReference;

    [Header("Portrait references")]
    public Image portraitLeftReference;
    public Image portraitRightReference;

    void Start()
    {
        GetComponent<DialogueController>().OnUpdateTextIndex += ChangeBackground;
        ChangeBackground(0);
    }

    public void ChangeBackground(uint index)
    {
        DialogueComponent currentDialogue = FindObjectOfType<DialogueHolder>().GetDialogueComponent(index);

        if(currentDialogue.background != null)
            backgroundReference.sprite = currentDialogue.background;


        if (currentDialogue.leftPortrait != null)
            portraitLeftReference.sprite = currentDialogue.leftPortrait;


        if (currentDialogue.rightPortrait != null)
            portraitRightReference.sprite = currentDialogue.rightPortrait;

    }
}

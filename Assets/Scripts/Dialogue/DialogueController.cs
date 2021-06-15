using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum DialogueMode { Click, Auto, Script }

[RequireComponent(typeof(DialogueIndex))]
[RequireComponent(typeof(DialogueName))]
public class DialogueController : MonoBehaviour
{
    [Header("Speed at which the dialogue is played at.")]
    public float regularInterval;
    public float dotInterval;
    public float commaInterval;
    public float dialoguePauseDuration;

    [Header("Text to display dialogue on.")]
    public Text dialogueText;
    public Text talkerText;

    [Header("Set dialogue mode.")]
    public DialogueMode dialogueMode;

    private DialogueHolder dialogueTextHolder;
    private DialogueIndex dialogueIndex;
    private DialogueName dialogueName;

    [Header("Pauses dialogue")]
    public bool isPaused;

    private uint currentTextIndex;
    private string finalText, currentText;
    private string talkerName;
    private uint currentCharacterIndex;
    private bool isDoneWriting, isDoneWithCharacter, isDoneWaiting, hasAborted;

    public event System.Action<uint> OnUpdateTextIndex = delegate { };
    public event System.Action<uint> OnFinishedText = delegate { };
    public event System.Action<bool> OnUpdateCharacter = delegate { };


    private void Awake()
    {
        if (FindObjectOfType<DialogueHolder>())
            dialogueTextHolder = FindObjectOfType<DialogueHolder>();
        else
        {
            Debug.LogError("Dialogue Holder Object could not be found in the scene!");
            this.enabled = false;
        }

        dialogueIndex = GetComponent<DialogueIndex>();
        dialogueName = GetComponent<DialogueName>();
    }

    private void Start()
    {
        isDoneWriting = true;
        isDoneWaiting = true;
        hasAborted = false;

        // Start dialogue trigger if on Click mode
        if (dialogueMode == DialogueMode.Click)
            StartCoroutine(StartNewDialogue());
    }

    private void Update()
    {
        if (isPaused)
            return;


        if (!isDoneWriting &&
            isDoneWaiting &&
            !hasAborted)
            NextCharacter();

        CheckMode();
    }

    public void PauseDialogue(bool enabled)
    {
        isPaused = enabled;
    }

    public bool GetPaused()
    {
        return isPaused;
    }

    public void NextDialogue()
    {
        ResetText();
        GetNewTextValues();
        ExecuteEventAction();
        OnUpdateTextIndex(currentTextIndex);

        isDoneWriting = false;
    }

    private void ExecuteEventAction()
    {
        dialogueTextHolder.GetEvent(currentTextIndex).Invoke();
    }

    private void GetNewTextValues()
    {
        dialogueIndex.IncrementIndex();
        currentTextIndex = dialogueIndex.GetCurrentIndex();
    
        if (currentTextIndex >= dialogueTextHolder.GetLength())
            Abort();

        finalText = dialogueName.FormatWithName(dialogueTextHolder.GetDialogueComponent(currentTextIndex).text);
        talkerName = dialogueName.FormatWithName(dialogueTextHolder.GetDialogueComponent(currentTextIndex).talker);

    }

    private void Abort()
    {
        SetText("");
        SetName("");
        hasAborted = true;
        enabled = false;
    }

    private void ResetText()
    {
        SetText("");
        SetName("");
        currentText = "";
        currentCharacterIndex = 0;
        isDoneWithCharacter = true;
    }

    private void NextCharacter()
    {
        SetName(talkerName);

        if ((uint)finalText.Length == currentCharacterIndex)
        {
            FinishSentence();
            return;
        }

        if (isDoneWithCharacter)
        {
            isDoneWithCharacter = false;
            StartCoroutine(LoadNextCharacter());
        }
    }

    private IEnumerator LoadNextCharacter()
    {
        char nextCharacter = finalText[(int)currentCharacterIndex];
        currentText += nextCharacter;
        SetText(currentText);

        currentCharacterIndex++;

        // OnUpdateCharacter "false" if character is readable (like abc)

        if (nextCharacter == ',')
        {
            OnUpdateCharacter(true);
            yield return new WaitForSeconds(commaInterval);
        }
        else if (nextCharacter == '.')
        {
            OnUpdateCharacter(true);
            yield return new WaitForSeconds(dotInterval);
        }
        else
        {
            OnUpdateCharacter(false);
            yield return new WaitForSeconds(regularInterval);
        }

        isDoneWithCharacter = true;
    }

    private void SetName(string name)
    {
        talkerText.text = name;
    }

    private void SetText(string text)
    {
        dialogueText.text = text;
    }

    private void CheckMode()
    {
        switch (dialogueMode)
        {
            case DialogueMode.Click:
                CheckForClick();
                break;
            case DialogueMode.Auto:
                AutomaticDialogue();
                break;
        }
    }

    private void FinishSentence()
    {
        SetText(finalText);
        isDoneWriting = true;

        OnFinishedText(currentCharacterIndex);
    }

    private void CheckForClick()
    {
        if (Input.GetMouseButtonDown(0))
            if (isDoneWriting)
                NextDialogue();
            else
                FinishSentence();
    }

    private void AutomaticDialogue()
    {
        if (isDoneWriting && isDoneWaiting)
        {
            isDoneWaiting = false;
            StartCoroutine(StartNewDialogue());
        }
    }

    private IEnumerator StartNewDialogue()
    {
        yield return new WaitForSeconds(dialoguePauseDuration);
        isDoneWaiting = true;
        NextDialogue();
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class DialogueHandler : MonoBehaviour
{
  // === PARAMS

  [Tooltip("How many seconds to wait between each character display")]
  public float writeDelay = 0.1f;

  [Tooltip("Initial font size, in pixels")]
  public float initialFontSize = 50f;

  [Tooltip("The initial dialogue to be displayed")]
  public Dialogue initialDialogue;

  [Tooltip("Extra pause after a comma")]
  public float commaTime = 0.2f;

  [Tooltip("Extra pause after a period")]
  public float periodTime = 0.5f;

  [Tooltip("Extra pause after a new line character")]
  public float newLineTime = 1.5f;

  // === STATE

  // The coroutine currently writing on screen
  Coroutine displayCoroutine;

  Dialogue _activeDialogue;

  // List of dialogues that were used
  List<Dialogue> usedDialogues;



  // === PROPERTIES

  // The currently displayed dialogue
  public Dialogue ActiveDialogue
  {
    get { return _activeDialogue; }

    private set
    {
      _activeDialogue = value;
      if (value != null) usedDialogues.Add(_activeDialogue);
    }

  }


  // === REFS

  Label dialogue;


  // === INTERFACE

  public Dialogue Load(string path)
  {
    return Resources.Load<Dialogue>($"Dialogues/{path}");
  }

  public void Interrupt()
  {
    // Detect interruption
    if (ActiveDialogue != null)
    {
      if (ActiveDialogue.Finished == false) ActiveDialogue.Interrupted = true;
      ActiveDialogue.RegisterLeave();
      ActiveDialogue = null;
    }

    if (displayCoroutine != null)
    {
      StopCoroutine(displayCoroutine);
      displayCoroutine = null;
    }

    dialogue.text = "";
  }

  public void Skip(bool setInterrupted = true)
  {
    if (ActiveDialogue == null || ActiveDialogue.skippable == false) return;

    Dialogue nextDialogue = ActiveDialogue.GetFollowUp();

    if (setInterrupted == false) ActiveDialogue.Finished = true;

    SetDialogue(nextDialogue);
  }

  public void SetDialogue(Dialogue dialogue)
  {
    // Interrupt a previous one if necessary
    Interrupt();

    ActiveDialogue = dialogue;

    if (dialogue != null) displayCoroutine = StartCoroutine(DisplayDialogue());
  }

  public void SetDialogue(string path) { SetDialogue(Load(path)); }

  public void InputSkip(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.performed) Skip();
  }


  // === INTERNAL

  private void Awake()
  {
    usedDialogues = new List<Dialogue>();
    dialogue = FindObjectOfType<UIDocument>().rootVisualElement.Q<Label>("dialogue");

    // Empty it
    dialogue.text = "";

    EnsureNotNull.Objects(dialogue);
  }

  private void Start()
  {
    if (ActiveDialogue == null && initialDialogue != null) SetDialogue(initialDialogue);
  }

  private void OnDisable()
  {
    foreach (var dialogue in usedDialogues) dialogue.Reset();
  }

  private IEnumerator DisplayDialogue()
  {
    while (ActiveDialogue != null)
    {
      // Set it as started
      ActiveDialogue.Started = true;

      // Wait delay
      if (ActiveDialogue.delay > 0f) yield return new WaitForSeconds(ActiveDialogue.delay);

      // Restart box
      dialogue.text = "";

      // Apply style
      ApplyDialogueStyle();

      // Get dialogue write speed
      float dialogueWriteSpeed = writeDelay * ActiveDialogue.speedModifier;

      // For each word
      var words = ActiveDialogue
        .text
        .Split(' ', StringSplitOptions.RemoveEmptyEntries);

      foreach (
        var (word, wordIndex) in words.Select((word, index) => (word, index))
      )
      {
        // Detect pauses
        if (Regex.IsMatch(word, @"^#\d+(.\d+)?$"))
        {
          yield return new WaitForSeconds(float.Parse(word.Substring(1)));

          continue;
        }

        // Detect early finished flag
        if (word == "#F" || word == "#f")
        {
          ActiveDialogue.Finished = true;

          continue;
        }

        // Detect special characters
        bool writingSpecialCharacters = false;

        // Type it plus a space (if not the first word)
        foreach (char c in $"{(wordIndex == 0 ? "" : " ")}{word}")
        {

          // Detect new lines
          if (c == '\n')
          {
            yield return new WaitForSeconds(newLineTime);
            dialogue.text = "";
            continue;
          }

          dialogue.text += c;

          // Detect special characters
          if (c == '<') writingSpecialCharacters = true;

          // Don't wait it writing special characters
          if (writingSpecialCharacters == false) yield return new WaitForSeconds(writeDelay);

          else if (c == '>') writingSpecialCharacters = false;
        }

        // Detect paused characters (if  not last word)
        if (wordIndex < words.Length - 1)
        {
          if (word[word.Length - 1] == ',') yield return new WaitForSeconds(commaTime);
          else if (".?!".Contains(word[word.Length - 1])) yield return new WaitForSeconds(periodTime);
        }
      }

      // Set it as finished
      ActiveDialogue.Finished = true;

      // Wait die time and stop
      if (ActiveDialogue.maxDuration > 0f)
      {
        yield return new WaitForSeconds(ActiveDialogue.maxDuration);
        dialogue.text = "";
        ActiveDialogue.RegisterLeave();
      }

      // Wait follow up time
      if (ActiveDialogue.followUpDelay > 0f)
        yield return new WaitForSeconds(ActiveDialogue.followUpDelay);

      // Follow up
      // No problem if register leave gets called twice
      ActiveDialogue.RegisterLeave();
      ActiveDialogue = ActiveDialogue.GetFollowUp();
    }

    displayCoroutine = null;
  }

  private void ApplyDialogueStyle()
  {
    dialogue.style.fontSize = new Length(ActiveDialogue.fontSizeModifier * initialFontSize);
  }
}

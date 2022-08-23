using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueHandler : MonoBehaviour
{
  // === PARAMS

  [Tooltip("How many seconds to wait between each character display")]
  public float writeSpeed = 0.2f;

  [Tooltip("The initial dialogue to be displayed")]
  public Dialogue initialDialogue;


  // === STATE

  // The currently displayed dialogue
  Dialogue activeDialogue;

  // How many characters  have already been displayed
  int displayedCharacters = 0;

  // The coroutine currently writing on screen
  Coroutine displayCoroutine;


  // === REFS

  Label dialogue;


  private void Awake()
  {
    dialogue = FindObjectOfType<UIDocument>().rootVisualElement.Q<Label>("dialogue");

    // Empty it
    dialogue.text = "";

    EnsureNotNull.Objects(dialogue);
  }

  private void Start()
  {
    if (activeDialogue == null && initialDialogue != null) SetDialogue(initialDialogue);
  }

  public void SetDialogue(Dialogue dialogue)
  {
    activeDialogue = dialogue;

    if (displayCoroutine != null) StopCoroutine(displayCoroutine);

    displayCoroutine = StartCoroutine(DisplayDialogue());
  }

  private IEnumerator DisplayDialogue()
  {
    float delayExecuted = 0f;

    while (activeDialogue != null)
    {
      // Get remaining delay
      float delay = activeDialogue.delay - delayExecuted;

      // Wait delay
      if (delay > 0f) yield return new WaitForSeconds(activeDialogue.delay);

      dialogue.text = "";

      // For each word
      foreach (string word in activeDialogue.text.Split(' '))
      {
        // Detect pauses
        if (Regex.IsMatch(word, @"^#\d+(.\d+)?$"))
        {
          yield return new WaitForSeconds(float.Parse(word.Substring(1)));

          continue;
        }

        // Type it plus a space (if not the first word)
        foreach (char c in $"{(dialogue.text.Length == 0 ? "" : " ")}{word}")
        {
          dialogue.text += c;

          yield return new WaitForSeconds(writeSpeed);
        }
      }

      // Detect follow up
      if (activeDialogue.followUp == null) break;

      // Wait follow up delay
      if (activeDialogue.followUpDelay > 0f)
        yield return new WaitForSeconds(activeDialogue.followUpDelay);

      delayExecuted = activeDialogue.followUpDelay;

      // Follow up
      activeDialogue = activeDialogue.followUp;
    }

    displayCoroutine = null;
  }
}

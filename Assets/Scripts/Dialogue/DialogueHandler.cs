using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueHandler : MonoBehaviour
{
  // === PARAMS

  [Tooltip("How many seconds to wait between each character display")]
  public float writeSpeed = 0.2f;


  // === STATE

  // The currently displayed dialogue
  Dialogue activeDialogue;

  // How many characters  have already been displayed
  int displayedCharacters = 0;

  // The coroutine currently writing on screen
  Coroutine writeCoroutine;


  public void SetDialogue(Dialogue dialogue)
  {
    activeDialogue = dialogue;

    if (writeCoroutine != null) StopCoroutine(writeCoroutine);

    writeCoroutine = StartCoroutine(DisplayDialogue());
  }

  private IEnumerator DisplayDialogue()
  {
    while (displayedCharacters++ < activeDialogue.text.Length)
    {
      print(activeDialogue.text.Substring(0, displayedCharacters));

      yield return new WaitForSeconds(writeSpeed);
    }

    writeCoroutine = null;
  }
}

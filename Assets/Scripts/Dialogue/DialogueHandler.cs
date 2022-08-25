using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
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

  string _displayedText;


  // === PROPERTIES

  // Text that is currently show on screen
  string DisplayedText
  {
    get { return _displayedText; }
    set
    {
      _displayedText = value;
      dialogue.text = value;
    }
  }

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
  public static DialogueHandler Instance { get; private set; }


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

    DisplayedText = "";
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


  // === INTERNAL

  private bool SingletonCheck()
  {
    if (Instance != null && Instance != this)
    {
      // Pass the params forward
      SendMessage("PassParamsAhead");

      // If there's already an instance, stop
      gameObject.SetActive(false);
      Destroy(gameObject);

      return false;
    }

    Instance = this;
    DontDestroyOnLoad(gameObject);

    return true;
  }

  private void Awake()
  {
    if (SingletonCheck() == false) return;

    usedDialogues = new List<Dialogue>();
    GetDialogueBox();
  }

  private void GetDialogueBox()
  {
    dialogue = FindObjectOfType<UIDocument>().rootVisualElement.Q<Label>("dialogue");

    EnsureNotNull.Objects(dialogue);

    // Empty it
    DisplayedText = "";
  }

  private void Start()
  {
    if (ActiveDialogue == null && initialDialogue != null) SetDialogue(initialDialogue);
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    if (usedDialogues != null) foreach (var dialogue in usedDialogues) dialogue.Reset();
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
  {
    // Get what was displayed
    string screenText = DisplayedText;

    GetDialogueBox();

    // Put it back there
    DisplayedText = screenText;
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
      DisplayedText = "";

      // Apply style
      ApplyDialogueStyle();

      // Get dialogue write speed
      float dialogueWriteSpeed = writeDelay / ActiveDialogue.speedModifier;
      float commaDelay = commaTime / ActiveDialogue.speedModifier;
      float periodDelay = periodTime / ActiveDialogue.speedModifier;
      float newLineDelay = newLineTime / ActiveDialogue.speedModifier;

      // For each word
      var words = ActiveDialogue.text.Split(' ').Select(word => word.Trim(new char[] { ' ' }))
        .Where(word => Regex.IsMatch(word, @"^[ ]*$") == false || word.Length == 1 && word[0] == '\n')
        .ToArray();

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
            yield return new WaitForSeconds(newLineDelay);
            DisplayedText = "";
            continue;
          }

          DisplayedText += c;

          // Detect special characters
          if (c == '<') writingSpecialCharacters = true;

          // Don't wait it writing special characters
          if (writingSpecialCharacters == false) yield return new WaitForSeconds(dialogueWriteSpeed);

          else if (c == '>') writingSpecialCharacters = false;
        }

        // Detect paused characters (if  not last word)
        if (wordIndex < words.Length - 1)
        {
          if (word[word.Length - 1] == ',') yield return new WaitForSeconds(commaDelay);
          else if (".?!".Contains(word[word.Length - 1])) yield return new WaitForSeconds(periodDelay);
        }
      }

      // Set it as finished
      ActiveDialogue.Finished = true;

      // Wait die time and stop
      if (ActiveDialogue.maxDuration > 0f)
      {
        yield return new WaitForSeconds(ActiveDialogue.maxDuration);
        DisplayedText = "";
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

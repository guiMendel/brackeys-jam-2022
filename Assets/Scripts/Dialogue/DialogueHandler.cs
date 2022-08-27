using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
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

  public bool destroyOnLoad = false;

  public AudioClip[] letterClips;

  public UnityEvent OnFinish;

  // === STATE

  // The coroutine currently writing on screen
  Coroutine displayCoroutine;

  Dialogue _activeDialogue;

  // List of dialogues that were used
  List<Dialogue> usedDialogues;

  string _displayedText;

  // Whether user has requested dialogue skip
  bool skipRequested = false;

  string[] tooltips = new string[0];



  // === PROPERTIES

  // Text that is currently show on screen
  string DisplayedText
  {
    get { return _displayedText; }
    set
    {
      if (dialogue == null) GetDialogueBox();

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
  AudioSource audioSource;
  VisualElement tooltipContainer => FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("tooltip-container");


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
    if (ActiveDialogue == null) return;

    DisplayedText = "";

    skipRequested = true;
  }

  public void SetDialogue(Dialogue dialogue)
  {
    // Interrupt a previous one if necessary
    Interrupt();

    ActiveDialogue = dialogue;

    if (dialogue != null) displayCoroutine = StartCoroutine(DisplayDialogue());
  }

  public void SetDialogue(string path) { SetDialogue(Load(path)); }

  public void ResetDialogues()
  {
    // Interrupt a previous one if necessary
    Interrupt();

    if (usedDialogues != null) foreach (var dialogue in usedDialogues) dialogue.Reset();
  }


  // === INTERNAL

  void SetTooltips() { SetTooltips(new string[] { }); }

  void SetTooltips(string[] newTooltips)
  {
    VisualElement tooltipContainer = FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("tooltip-container");

    tooltipContainer.Clear();

    foreach (var tooltip in newTooltips)
    {
      tooltipContainer.Add(new Label(tooltip));
    }

    tooltips = newTooltips;
  }

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

    if (destroyOnLoad == false)
    {
      DontDestroyOnLoad(gameObject);
      Instance = this;
    }

    return true;
  }

  private void Awake()
  {
    if (SingletonCheck() == false) return;

    usedDialogues = new List<Dialogue>();
    audioSource = GetComponent<AudioSource>();

    EnsureNotNull.Objects(audioSource);

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
    ResetDialogues();
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
  {
    // Get what was displayed
    string screenText = DisplayedText;


    GetDialogueBox();

    // Put it back there
    DisplayedText = screenText;

    // Gat what the tooltips were
    if (tooltips.Length > 0) SetTooltips(tooltips);
  }

  private IEnumerator DisplayDialogue()
  {
    while (ActiveDialogue != null)
    {
      // Reset skip request
      skipRequested = false;

      // Set it as started
      ActiveDialogue.Started = true;

      // Wait delay
      if (ActiveDialogue.delay > 0f) yield return new WaitForSecondsRealtime(ActiveDialogue.delay);

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
        var wordPair in words.Select((word, index) => (word, index))
      )
      {
        string word = wordPair.word;
        int wordIndex = wordPair.index;

        if (skipRequested)
        {
          // On skip requested, seek next line break or next dialogue, whichever comes first
          if (word.Contains('\n') == false) continue;

          // Trim everything before the line break
          DisplayedText = "";
          skipRequested = false;
          if (word.Length == 1) continue;
          word = word.Substring(word.IndexOf('\n') + 1);
        }

        // Detect pauses
        if (Regex.IsMatch(word, @"^#\d+(.\d+)?$"))
        {
          yield return new WaitForSecondsRealtime(float.Parse(word.Substring(1)));

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
          if (skipRequested) break;

          // Detect new lines
          if (c == '\n')
          {
            // If not auto skipping, wait for skip
            if (ActiveDialogue.autoSkip == false)
            {
              yield return new WaitUntil(() => skipRequested);
              skipRequested = false;
            }

            else yield return new WaitForSecondsRealtime(newLineDelay);

            DisplayedText = "";
            continue;
          }

          DisplayedText += c;

          PlayLetterClip();

          // Detect special characters
          if (c == '<') writingSpecialCharacters = true;

          // Don't wait it writing special characters
          if (writingSpecialCharacters == false) yield return new WaitForSecondsRealtime(dialogueWriteSpeed);

          else if (c == '>') writingSpecialCharacters = false;
        }

        // Detect paused characters (if  not last word)
        if (wordIndex < words.Length - 1)
        {
          if (word[word.Length - 1] == ',') yield return new WaitForSecondsRealtime(commaDelay);
          else if (".?!".Contains(word[word.Length - 1])) yield return new WaitForSecondsRealtime(periodDelay);
        }
      }

      // Set it as finished
      ActiveDialogue.Finished = true;

      // If not auto skipping
      if (ActiveDialogue.autoSkip == false)
      {
        yield return new WaitUntil(() => skipRequested);
        ActiveDialogue.RegisterLeave();
      }

      // Wait die time and stop
      else if (ActiveDialogue.maxDuration > 0f)
      {
        if (skipRequested == false) yield return new WaitForSecondsRealtime(ActiveDialogue.maxDuration);
        DisplayedText = "";
        ActiveDialogue.RegisterLeave();
      }

      // Wait follow up time
      if (!skipRequested && ActiveDialogue.followUpDelay > 0f)
        yield return new WaitForSecondsRealtime(ActiveDialogue.followUpDelay);

      // Follow up
      // No problem if register leave gets called twice
      ActiveDialogue.RegisterLeave();
      ActiveDialogue = ActiveDialogue.GetFollowUp();

      OnFinish.Invoke();
    }

    displayCoroutine = null;
  }

  private void PlayLetterClip()
  {
    AudioClip clip = letterClips[Random.Range(0, letterClips.Length)];

    // audioSource.clip = clip;
    // audioSource.Play();
    audioSource.PlayOneShot(clip);
  }

  private void ApplyDialogueStyle()
  {
    dialogue.style.fontSize = new Length(ActiveDialogue.fontSizeModifier * initialFontSize);
    audioSource.pitch = ActiveDialogue.pitchModifier;

    if (ActiveDialogue.resetTooltips) SetTooltips();

    if (ActiveDialogue.setTooltips != null && ActiveDialogue.setTooltips.Length > 0)
    {
      SetTooltips(ActiveDialogue.setTooltips);
    }
  }
}

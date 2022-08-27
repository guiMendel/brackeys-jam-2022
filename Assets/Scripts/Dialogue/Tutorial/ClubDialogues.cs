using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClubDialogues : MonoBehaviour
{
  // === PARAMS

  public float deathDialogueChance = 0.5f;


  // === REFS

  PlayerController playerController => FindObjectOfType<PlayerController>();
  LaserVulnerable playerVulnerable => playerController?.GetComponentInChildren<LaserVulnerable>();
  DialogueHandler handler => GetComponent<DialogueHandler>();
  SuspicionMeter suspicionMeter => FindObjectOfType<SuspicionMeter>();
  SuspicionMeterFiller meterFiller => FindObjectOfType<SuspicionMeterFiller>();


  // === STATE

  Dialogue[] deathDialogues;

  List<Dialogue> unplayedDeathDialogues = new List<Dialogue>();


  // === DIALOGUES

  Dialogue intro;
  Dialogue rememberLives;
  Dialogue allTheSame;
  Dialogue believe;
  Dialogue doesItHurt;
  Dialogue favoriteCharacter;
  Dialogue goodFeeling;
  Dialogue goodShots;
  Dialogue hateAlien;
  Dialogue howWeHere;
  Dialogue noTrust;
  Dialogue restrooms;
  Dialogue spotlight;
  Dialogue stink;
  Dialogue tooMuch;


  private void Awake()
  {
    intro = handler.Load("Club/1Intro");
    rememberLives = handler.Load("Club/RememberLives");
    allTheSame = handler.Load("Club/AllTheSame");
    believe = handler.Load("Club/Believe");
    doesItHurt = handler.Load("Club/DoesItHurt");
    favoriteCharacter = handler.Load("Club/FavoriteCharacter");
    goodFeeling = handler.Load("Club/GoodFeeling");
    goodShots = handler.Load("Club/GoodShots");
    hateAlien = handler.Load("Club/HateAlien");
    howWeHere = handler.Load("Club/HowWeHere");
    noTrust = handler.Load("Club/NoTrust");
    restrooms = handler.Load("Club/Restrooms");
    spotlight = handler.Load("Club/Spotlight");
    stink = handler.Load("Club/Stink");
    tooMuch = handler.Load("Club/TooMuch");

    deathDialogues = new Dialogue[] {
      allTheSame, believe, doesItHurt, favoriteCharacter, goodFeeling, goodShots,
      hateAlien, howWeHere, noTrust, restrooms, spotlight, stink, tooMuch, rememberLives
    };

    unplayedDeathDialogues = deathDialogues.ToList();

    TutorialDialogues tutorial = GetComponent<TutorialDialogues>();

    if (tutorial != null) Destroy(tutorial);

    EnsureNotNull.Objects(
      intro, deathDialogues
    );
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
  {
    if (scene.name == "EndGame")
    {
      DialogueHandler.Instance.SetDialogue("EndGame/Farewell");

      Destroy(FindObjectOfType<SceneHandler>());

      return;
    }

    playerVulnerable.OnDeath.AddListener(PlayDeathDialogue);
  }

  private void PlayDeathDialogue()
  {
    if (Random.value > deathDialogueChance || handler.ActiveDialogue != null) return;

    int dialogueIndex = Random.Range(0, unplayedDeathDialogues.Count);

    handler.SetDialogue(unplayedDeathDialogues[dialogueIndex]);
    unplayedDeathDialogues.RemoveAt(dialogueIndex);
  }

  void PassParamsAhead()
  {
    DialogueHandler handler = DialogueHandler.Instance;

    if (handler.GetComponent<ClubDialogues>() != null) return;

    handler.AddComponent<ClubDialogues>();
    handler.SetDialogue("Club/1Intro");
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialDialogues : MonoBehaviour
{
  // === PARAMS

  public ScreenTransition screen2Transition;


  // === REFS

  PlayerController playerController => FindObjectOfType<PlayerController>();
  LaserVulnerable playerVulnerable => playerController?.GetComponentInChildren<LaserVulnerable>();
  DialogueHandler handler => GetComponent<DialogueHandler>();
  SuspicionMeter suspicionMeter => FindObjectOfType<SuspicionMeter>();
  SuspicionMeterFiller meterFiller => FindObjectOfType<SuspicionMeterFiller>();


  // === DIALOGUES

  Dialogue intro;
  Dialogue movement;
  Dialogue advanceScreen;
  Dialogue rushedAdvanceScreen;
  Dialogue newScreen;
  Dialogue rushedNewScreen;
  Dialogue pullLever;
  Dialogue freakOut;
  Dialogue explainAliens;
  Dialogue tryLeverAgain;
  Dialogue explainTimeTravel;
  Dialogue solvePuzzle;


  protected void Awake()
  {
    movement = handler.Load("Tutorial/2Movement");
    advanceScreen = handler.Load("Tutorial/3AdvanceScreen");
    rushedAdvanceScreen = handler.Load("Tutorial/3RushedAdvanceScreen");
    intro = handler.Load("Tutorial/1Intro");
    newScreen = handler.Load("Tutorial/4NewScreen");
    rushedNewScreen = handler.Load("Tutorial/4RushedNewScreen");
    pullLever = handler.Load("Tutorial/5PullLever");
    freakOut = handler.Load("Tutorial/6FreakOutAboutAliens");
    explainAliens = handler.Load("Tutorial/7ExplainAliens");
    tryLeverAgain = handler.Load("Tutorial/8TryLeverAgain");
    explainTimeTravel = handler.Load("Tutorial/9ExplainTimeTravel");
    solvePuzzle = handler.Load("Tutorial/10SolvePuzzle");

    EnsureNotNull.Objects(
      advanceScreen, movement, rushedAdvanceScreen, intro, screen2Transition, newScreen, pullLever,
      freakOut, tryLeverAgain, explainAliens, explainTimeTravel, solvePuzzle
    );
  }

  void FirstMoveListener(Vector2 direction)
  {
    if (movement.Finished == false) handler.SetDialogue(rushedAdvanceScreen);
    else handler.SetDialogue(advanceScreen);

    // Unsubscribe
    playerController.OnPlayerMove.RemoveListener(FirstMoveListener);
  }

  private void OnEnable()
  {
    intro.OnLeave.AddListener(StartTutorial);
    screen2Transition.OnTransitionStart.AddListener(TransitionDialogue);
    pullLever.OnStart.AddListener(SetUpLeverBit);
    suspicionMeter.OnAggro.AddListener(FreakOutDialogue);
    SceneManager.sceneLoaded += OnSceneLoaded;
    tryLeverAgain.OnStart.AddListener(EnableSuspicionMeter);
    solvePuzzle.OnStop.AddListener(EnableSuspicionMeter);
  }

  private void OnDisable()
  {
    intro?.OnLeave.RemoveListener(StartTutorial);
    screen2Transition?.OnTransitionStart?.RemoveListener(TransitionDialogue);
    pullLever?.OnStart.RemoveListener(SetUpLeverBit);
    suspicionMeter?.OnAggro.RemoveListener(FreakOutDialogue);
    SceneManager.sceneLoaded -= OnSceneLoaded;
    tryLeverAgain?.OnStart.RemoveListener(EnableSuspicionMeter);
    playerVulnerable?.OnDeath.RemoveListener(ExplainTimeTravel);
    solvePuzzle?.OnStop.RemoveListener(EnableSuspicionMeter);
  }

  private void ExplainTimeTravel()
  {
    // Only trigger after a certain dialogue
    if (tryLeverAgain.Started == false || explainTimeTravel.Started) return;

    handler.SetDialogue(explainTimeTravel);
  }

  private void EnableSuspicionMeter()
  {
    // Allow suspicion meter
    suspicionMeter.enabled = true;
    meterFiller.hideMeter = false;

    // Also give player movement agency
    FindObjectOfType<PlayerController>().disableMovement = false;
  }

  private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
  {
    // The first time, load this dialogue
    if (explainAliens.Started == false) handler.SetDialogue(explainAliens);

    // After first screen, open curtains on scene load
    FindObjectOfType<UICurtain>().openOnLoad = true;

    if (
      (tryLeverAgain.Started && explainTimeTravel.Started == false)
      || solvePuzzle.Stopped
    ) EnableSuspicionMeter();

    playerVulnerable.OnDeath.AddListener(ExplainTimeTravel);
  }

  private void FreakOutDialogue()
  {
    handler.SetDialogue(freakOut);

    suspicionMeter.OnAggro.RemoveListener(FreakOutDialogue);
  }

  private void SetUpLeverBit()
  {
    // Restore control
    playerController.disableMovement = false;

    // Activate alien suspicion
    suspicionMeter.enabled = true;
  }

  private void TransitionDialogue()
  {
    // Disable movement
    playerController.disableMovement = true;

    if (advanceScreen.Finished || rushedAdvanceScreen.Finished) handler.SetDialogue(newScreen);

    else handler.SetDialogue(rushedNewScreen);
  }

  private void StartTutorial()
  {
    // Open curtains
    FindObjectOfType<UICurtain>().Open();

    // Give player control over movement
    playerController.disableMovement = false;
  }

  protected void Start()
  {
    // On first player input, display next dialogue phase
    playerController.OnPlayerMove.AddListener(FirstMoveListener);

    suspicionMeter.enabled = false;
    meterFiller.hideMeter = true;
  }
}

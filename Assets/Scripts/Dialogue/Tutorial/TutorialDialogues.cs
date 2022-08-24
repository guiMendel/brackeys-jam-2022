using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialDialogues : MonoBehaviour
{
  // === PARAMS

  public ScreenTransition screen2Transition;


  // === REFS

  PlayerController playerController;
  DialogueHandler handler;
  SuspicionMeter suspicionMeter;
  SuspicionMeterFiller meterFiller;


  // === DIALOGUES

  Dialogue intro;
  Dialogue movement;
  Dialogue advanceScreen;
  Dialogue rushedAdvanceScreen;
  Dialogue newScreen;
  Dialogue rushedNewScreen;
  Dialogue pullLever;


  protected void Awake()
  {
    playerController = FindObjectOfType<PlayerController>();
    handler = GetComponent<DialogueHandler>();
    suspicionMeter = FindObjectOfType<SuspicionMeter>();
    meterFiller = FindObjectOfType<SuspicionMeterFiller>();

    movement = handler.Load("Tutorial/2Movement");
    advanceScreen = handler.Load("Tutorial/3AdvanceScreen");
    rushedAdvanceScreen = handler.Load("Tutorial/3RushedAdvanceScreen");
    intro = handler.Load("Tutorial/1Intro");
    newScreen = handler.Load("Tutorial/4NewScreen");
    rushedNewScreen = handler.Load("Tutorial/4RushedNewScreen");
    pullLever = handler.Load("Tutorial/5PullLever");

    EnsureNotNull.Objects(
      handler, playerController, advanceScreen, movement,
      rushedAdvanceScreen, intro, screen2Transition, newScreen, pullLever,
      suspicionMeter, meterFiller
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
  }

  private void OnDisable()
  {
    intro.OnLeave.RemoveListener(StartTutorial);
    screen2Transition.OnTransitionStart.RemoveListener(TransitionDialogue);
    pullLever.OnStart.RemoveListener(SetUpLeverBit);
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

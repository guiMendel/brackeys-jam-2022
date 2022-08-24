using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TutorialDialogues : MonoBehaviour
{
  // === REFS

  PlayerController playerController;
  DialogueHandler handler;


  // === DIALOGUES

  Dialogue intro;
  Dialogue movement;
  Dialogue advanceScreen;
  Dialogue rushedAdvanceScreen;


  protected void Awake()
  {
    playerController = FindObjectOfType<PlayerController>();
    handler = GetComponent<DialogueHandler>();

    movement = handler.Load("Tutorial/2Movement");
    advanceScreen = handler.Load("Tutorial/3AdvanceScreen");
    rushedAdvanceScreen = handler.Load("Tutorial/3RushedAdvanceScreen");
    intro = handler.Load("Tutorial/1Intro");

    EnsureNotNull.Objects(
      handler, playerController, advanceScreen, movement, rushedAdvanceScreen, intro
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
  }

  private void OnDisable()
  {
    intro.OnLeave.RemoveListener(StartTutorial);
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
  }
}

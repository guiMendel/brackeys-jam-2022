using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  // === REFS

  Movement movement;

  private void Awake()
  {
    movement = GetComponent<Movement>();

    EnsureNotNull.Objects(movement);
  }


  public void Move(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.started) return;

    movement.SetTargetMovement(callbackContext.ReadValue<Vector2>());
  }
}

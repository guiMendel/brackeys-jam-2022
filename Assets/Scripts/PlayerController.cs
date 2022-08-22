using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("Player acceleration, in maxSpeed proportion per second")]
  public float acceleration = 10f;

  [Tooltip("Player max speed")]
  public float maxSpeed = 8;


  // === STATE

  Vector2 movementDirection;


  // === REFS

  Rigidbody2D body;


  private void Awake()
  {
    body = GetComponent<Rigidbody2D>();

    EnsureNotNull.Objects(body);
  }

  private void Update()
  {
    float scaledAcceleration = acceleration * Time.deltaTime;

    // Set movement
    body.velocity = movementDirection * maxSpeed * scaledAcceleration + body.velocity * (1 - scaledAcceleration);
  }

  public void Move(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.started) return;

    movementDirection = callbackContext.ReadValue<Vector2>();
  }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCharacter : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Acceleration when sprinting")]
  public float sprintAcceleration = 13f;

  [Tooltip("Max speed when sprinting")]
  public float sprintMaxSpeed = 4.5f;


  // === STATE

  float walkAcceleration;
  float walkMaxSpeed;


  // === PROPERTIES

  // Whether is sprinting
  public bool IsSprinting { get; private set; } = false;


  // === REFS

  Movement movement;
  Animator animator;

  private void Awake()
  {
    movement = GetComponent<Movement>();
    animator = GetComponent<Animator>();

    EnsureNotNull.Objects(movement, animator);

    // Register initial movement config
    walkAcceleration = movement.acceleration;
    walkMaxSpeed = movement.maxSpeed;
  }


  public void Move(Vector2 direction)
  {
    if (direction == Vector2.zero) animator.Play("idle");
    else if (IsSprinting) animator.Play("sprint");
    else animator.Play("walk");

    movement.SetTargetMovement(direction);
  }

  public void Sprint(bool sprint)
  {
    IsSprinting = sprint;

    // Adjust movement config
    if (IsSprinting)
    {
      movement.acceleration = sprintAcceleration;
      movement.maxSpeed = sprintMaxSpeed;
    }
    else
    {
      movement.acceleration = walkAcceleration;
      movement.maxSpeed = walkMaxSpeed;
    }
  }
}

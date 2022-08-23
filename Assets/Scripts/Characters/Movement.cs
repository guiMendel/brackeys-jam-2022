using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("Acceleration, in maxSpeed proportion per second")]
  public float acceleration = 13f;

  [Tooltip("Max speed")]
  public float maxSpeed = 4.5f;


  // === STATE

  public Vector2 MovementDirection { get; private set; }
  SpriteRenderer spriteRenderer;


  // === REFS

  Rigidbody2D body;


  // === INTERFACE

  public void SetTargetMovement(Vector2 movementDirection)
  {
    MovementDirection = movementDirection;
  }


  private void Awake()
  {
    body = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();

    EnsureNotNull.Objects(body, spriteRenderer);
  }

  private void Start()
  {
    // Init render order
    SetRenderOrder();
  }

  private void Update()
  {
    Move();
  }

  private void SetRenderOrder()
  {
    spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
  }

  private void Move()
  {
    // Acceleration with time scale
    float scaledAcceleration = acceleration * Time.deltaTime;

    // Number to approximate the speed to
    Vector2 movementTarget = MovementDirection * maxSpeed;

    // If already there, do nothing
    if (movementTarget == body.velocity) return;

    // Set movement
    body.velocity = movementTarget * scaledAcceleration + body.velocity * (1 - scaledAcceleration);

    // Update render order
    SetRenderOrder();
  }
}

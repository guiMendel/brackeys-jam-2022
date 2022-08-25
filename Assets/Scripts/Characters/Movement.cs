using System;
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

  [Tooltip("Whether to use a rigidbody")]
  public bool useRigidbody2D = true;

  [Tooltip("Whether to adjust sprite render order")]
  public bool adjustSpriteRenderingOrder = true;

  [Tooltip("Whether to adjust local scale to match movement direction")]
  public bool adjustScaleDirection = true;

  [Tooltip("Snap distance")]
  public float snapDistance = 0.05f;


  // === STATE

  public Vector2 MovementDirection { get; private set; }

  Vector2 _nonRigidbodySpeed = Vector2.zero;

  Coroutine moveToCoroutine;


  // === PROPERTIES

  public Vector2 Speed
  {
    get { return useRigidbody2D ? body.velocity : _nonRigidbodySpeed; }
    private set
    {
      if (useRigidbody2D) body.velocity = value;
      else _nonRigidbodySpeed = value;
    }
  }


  // === REFS

  Rigidbody2D body;
  SpriteRenderer spriteRenderer;


  // === INTERFACE

  public void SetTargetMovement(Vector2 movementDirection) { SetTargetMovement(movementDirection, true); }

  public Coroutine MoveTo(Vector2 targetPosition)
  {
    StopMoveTo();

    moveToCoroutine = StartCoroutine(MoveToCoroutine(targetPosition));

    return moveToCoroutine;
  }

  public void SnapTo(Vector2 position)
  {
    Speed = Vector2.zero;
    MovementDirection = Vector2.zero;
    transform.position = new Vector3(position.x, position.y, transform.position.z);
  }


  private void Awake()
  {
    body = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();

    if (useRigidbody2D) EnsureNotNull.Objects(body);
    if (adjustSpriteRenderingOrder) EnsureNotNull.Objects(spriteRenderer);
  }

  private IEnumerator MoveToCoroutine(Vector2 targetPosition)
  {
    // Gets distance to target
    Vector2 TargetDistance() { return (targetPosition - (Vector2)transform.position); }

    while (TargetDistance().sqrMagnitude > snapDistance * snapDistance)
    {
      // Set speed
      SetTargetMovement(TargetDistance().normalized, stopMoveTo: false);

      yield return new WaitForEndOfFrame();
    }

    // Snap
    SnapTo(targetPosition);

    moveToCoroutine = null;
  }

  private void SetTargetMovement(Vector2 movementDirection, bool stopMoveTo)
  {
    MovementDirection = movementDirection;

    if (stopMoveTo) StopMoveTo();
  }

  private void StopMoveTo()
  {
    if (moveToCoroutine == null) return;

    StopCoroutine(moveToCoroutine);
    moveToCoroutine = null;
  }

  private void Start()
  {
    // Init render order
    SetRenderOrder();
  }

  private void Update()
  {
    Move();

    if (useRigidbody2D == false) NonRigidbodyMove();
  }

  private void NonRigidbodyMove()
  {
    transform.position += new Vector3(Speed.x, Speed.y, 0f) * Time.deltaTime;
  }

  private void SetRenderOrder()
  {
    if (adjustSpriteRenderingOrder == false) return;

    spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
  }

  private void Move()
  {
    // Acceleration with time scale
    float scaledAcceleration = acceleration * Time.deltaTime;

    // Number to approximate the speed to
    Vector2 movementTarget = MovementDirection * maxSpeed;

    // If already there, do nothing
    if (movementTarget == Speed) return;

    // Set movement
    Speed = movementTarget * scaledAcceleration + Speed * (1 - scaledAcceleration);

    // Update render order
    SetRenderOrder();

    // Update scale direction
    SetScaleDirection();
  }

  private void SetScaleDirection()
  {
    // Ignore if moving very little
    if (adjustScaleDirection == false || Mathf.Abs(MovementDirection.x) < 0.2f) return;

    if (MovementDirection.x < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
    else transform.localScale = Vector3.one;
  }
}

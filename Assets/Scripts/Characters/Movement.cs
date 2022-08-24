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


  // === STATE

  public Vector2 MovementDirection { get; private set; }

  Vector2 _nonRigidbodySpeed = Vector2.zero;


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

  public void SetTargetMovement(Vector2 movementDirection)
  {
    MovementDirection = movementDirection;
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
  }
}

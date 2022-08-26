using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Movement : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("Acceleration, in maxSpeed proportion per second")]
  public float acceleration = 13f;

  [Tooltip("Max speed")]
  public float maxSpeed = 4.5f;

  [Tooltip("Whether to use a rigidbody")]
  public bool useRigidbody2D = true;

  [Tooltip("Whether to use navMesh when there isn't a clear path directly to the target")]
  public bool useNavMesh = false;

  [Tooltip("Whether to adjust sprite render order")]
  public bool adjustSpriteRenderingOrder = true;

  [Tooltip("Whether to adjust local scale to match movement direction")]
  public bool adjustScaleDirection = true;

  [Tooltip("Snap distance")]
  public float snapDistance = 0.05f;


  // === STATE

  public Vector2 MovementDirection { get; private set; }

  Vector2 _nonRigidbodySpeed = Vector2.zero;

  Vector2? destination;

  // Whether control over the object's movement was conceded to the navMesh
  bool controlDisabled = false;


  // === PROPERTIES

  public Vector2 Speed
  {
    get
    {
      if (useNavMesh && navMeshAgent.enabled) return navMeshAgent.velocity;
      if (useRigidbody2D) return body.velocity;
      return _nonRigidbodySpeed;
    }
    private set
    {
      if (useNavMesh && navMeshAgent.enabled) navMeshAgent.velocity = value;
      else if (useRigidbody2D) body.velocity = value;
      else _nonRigidbodySpeed = value;
    }
  }


  // === REFS

  Rigidbody2D body;
  SpriteRenderer spriteRenderer;
  NavMeshAgent navMeshAgent;
  CircleCollider2D circleCollider2D;


  // === INTERFACE

  public void SetTargetMovement(Vector2 movementDirection) { SetTargetMovement(movementDirection, true); }

  public void MoveTo(Vector2 targetPosition)
  {
    destination = targetPosition;
  }

  public void SnapTo(Vector2 position)
  {
    Speed = Vector2.zero;
    MovementDirection = Vector2.zero;

    Vector3 newPosition = new Vector3(position.x, position.y, transform.position.z);

    if (useNavMesh) navMeshAgent.Warp(newPosition);
    else transform.position = newPosition;
  }


  private void Awake()
  {
    body = GetComponent<Rigidbody2D>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    navMeshAgent = GetComponent<NavMeshAgent>();
    circleCollider2D = GetComponent<CircleCollider2D>();

    if (useRigidbody2D) EnsureNotNull.Objects(body);
    if (adjustSpriteRenderingOrder) EnsureNotNull.Objects(spriteRenderer);
    if (useNavMesh)
    {
      EnsureNotNull.Objects(navMeshAgent, circleCollider2D);

      navMeshAgent.updateRotation = false;
      navMeshAgent.updateUpAxis = false;
      navMeshAgent.acceleration = acceleration;
      navMeshAgent.speed = maxSpeed;
    }
  }

  private void SetTargetMovement(Vector2 movementDirection, bool stopMoveTo)
  {
    MovementDirection = movementDirection;

    if (stopMoveTo) destination = null;
    controlDisabled = false;
  }

  private void Start()
  {
    // Init render order
    SetRenderOrder();
  }

  private void Update()
  {
    // Update move direction, if with a destination
    FollowDestination();

    // Update speed
    Accelerate();

    // If not using rigid body, manually displace the position
    if (useRigidbody2D == false) DisplacePosition();

    // Update render order
    SetRenderOrder();

    // Update scale direction
    SetScaleDirection();
  }

  private void FollowDestination()
  {
    if (destination == null) return;

    // Gets distance to target
    Vector2 targetDistance = destination.Value - (Vector2)transform.position;

    // If no direct path AND can use navMesh, use it
    // if (useNavMesh)
    if (useNavMesh && DirectPathOver(targetDistance) == false)
    {
      // Cancel this script's influence over movement
      controlDisabled = true;

      // Use navMesh
      navMeshAgent.SetDestination(destination.Value);

      return;
    }

    // Otherwise, use own movement
    controlDisabled = false;

    if (useNavMesh) navMeshAgent.ResetPath();

    // Check if arrived
    if (targetDistance.sqrMagnitude <= snapDistance * snapDistance)
    {
      // Snap
      SnapTo(destination.Value);

      // Done
      destination = null;

      return;
    }

    // Set speed
    SetTargetMovement(targetDistance.normalized, stopMoveTo: false);
  }

  private bool DirectPathOver(Vector2 path)
  {
    return circleCollider2D.Cast(path, new RaycastHit2D[1], path.magnitude, true) == 0;
  }

  private void DisplacePosition()
  {
    if (controlDisabled) return;

    transform.position += new Vector3(Speed.x, Speed.y, 0f) * Time.deltaTime;
  }

  private void SetRenderOrder()
  {
    if (adjustSpriteRenderingOrder == false) return;

    spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
  }

  private void Accelerate()
  {
    if (controlDisabled) return;

    // Acceleration with time scale
    float scaledAcceleration = acceleration * Time.deltaTime;

    // Number to approximate the speed to
    Vector2 movementTarget = MovementDirection * maxSpeed;

    // If already there, do nothing
    if (movementTarget == Speed) return;

    // Set movement
    Speed = movementTarget * scaledAcceleration + Speed * (1 - scaledAcceleration);
  }

  private void SetScaleDirection()
  {
    // Ignore if moving very little
    if (adjustScaleDirection == false || Mathf.Abs(Speed.x) < 0.2f) return;

    if (Speed.x < 0f) transform.localScale = new Vector3(-1f, 1f, 1f);
    else transform.localScale = Vector3.one;
  }
}

using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnAnimationScript : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Where to walk to once spawned")]
  public Transform destination;

  [Tooltip("How far to snap from")]
  public float snapDistance = 0.05f;


  // === REFS

  Movement movement;
  Rigidbody2D body;
  NpcController controller;


  private void Awake()
  {
    movement = GetComponent<Movement>();
    body = GetComponent<Rigidbody2D>();
    controller = GetComponent<NpcController>();

    EnsureNotNull.Objects(movement, body);
  }

  void Start()
  {
    // Disable the character's collisions
    movement.SnapTo(movement.transform.position);
    body.bodyType = RigidbodyType2D.Static;
    movement.useRigidbody2D = false;
    if (controller != null) controller.enabled = false;
  }

  private void Update()
  {
    // Get target direction
    Vector2 targetDistance = ((Vector2)destination.position - (Vector2)transform.position);

    // Set his speed
    movement.SetTargetMovement(targetDistance.normalized);

    CheckArrived(targetDistance);
  }

  private void CheckArrived(Vector2 targetDistance)
  {
    float sqrDistance = targetDistance.sqrMagnitude;

    if (sqrDistance > snapDistance * snapDistance) return;

    // Snap
    movement.SnapTo(destination.position);

    // Return character properties
    ReturnProperties();

    // Stop
    enabled = false;
    Destroy(this);
  }

  private void ReturnProperties()
  {
    body.bodyType = RigidbodyType2D.Dynamic;
    movement.useRigidbody2D = true;
    if (controller != null) controller.enabled = true;
  }
}

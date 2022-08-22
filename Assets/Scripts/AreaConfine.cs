using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaConfine : MonoBehaviour
{

  // === INTERFACE

  [Tooltip("Area to confine to")]
  public BoxCollider2D movementArea;

  [Tooltip("Whenever this object is out of the confinement area, this event is raised, and passes a vector pointing to the area.")]
  public Event.Vector2 OnLeaveConfinement;


  // === REFS

  BoxCollider2D ownCollider;


  private void Awake()
  {
    ownCollider = GetComponent<BoxCollider2D>();

    OnLeaveConfinement ??= new Event.Vector2();

    EnsureNotNull.Objects(ownCollider, OnLeaveConfinement);
  }

  private void Update()
  {
    // The direction of the area
    Vector2 areaDirection = Vector2.zero;

    if (ownCollider.bounds.min.x < movementArea.bounds.min.x) areaDirection += Vector2.right;

    if (ownCollider.bounds.max.x > movementArea.bounds.max.x) areaDirection += Vector2.left;

    if (ownCollider.bounds.max.y > movementArea.bounds.max.y) areaDirection += Vector2.down;

    if (ownCollider.bounds.min.y < movementArea.bounds.min.y) areaDirection += Vector2.up;

    // Dont' raise if not out of area
    if (areaDirection == Vector2.zero) return;

    OnLeaveConfinement.Invoke(areaDirection);
  }
}

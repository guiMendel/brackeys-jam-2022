using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AreaConfine : MonoBehaviour
{

  // === INTERFACE

  [Tooltip("Area to confine to")]
  public BoxCollider2D movementArea;

  [Tooltip("Whenever this object is out of the confinement area, this event is raised, and passes a vector with the distance to the area.")]
  public Event.Vector2 OnOutsideConfinement;

  [Tooltip("When leaves")]
  public UnityEvent OnLeaveConfinement;

  [Tooltip("When returns")]
  public UnityEvent OnEnterConfinement;


  // === STATE

  bool isOutside = false;


  // === REFS

  BoxCollider2D ownCollider;


  private void Awake()
  {
    ownCollider = GetComponent<BoxCollider2D>();

    OnOutsideConfinement ??= new Event.Vector2();
    OnEnterConfinement ??= new UnityEvent();
    OnLeaveConfinement ??= new UnityEvent();

    EnsureNotNull.Objects(ownCollider, OnOutsideConfinement, OnEnterConfinement, OnLeaveConfinement);
  }

  private void Update()
  {
    // The direction of the area
    Vector2 areaDirection = Vector2.zero;

    if (ownCollider.bounds.min.x < movementArea.bounds.min.x)
      areaDirection += Vector2.right * (movementArea.bounds.min.x - ownCollider.bounds.min.x);

    if (ownCollider.bounds.max.x > movementArea.bounds.max.x)
      areaDirection += Vector2.left * (ownCollider.bounds.max.x - movementArea.bounds.max.x);

    if (ownCollider.bounds.max.y > movementArea.bounds.max.y)
      areaDirection += Vector2.down * (ownCollider.bounds.max.y - movementArea.bounds.max.y);

    if (ownCollider.bounds.min.y < movementArea.bounds.min.y)
      areaDirection += Vector2.up * (movementArea.bounds.min.y - ownCollider.bounds.min.y);

    // Dont' raise if not out of area
    if (areaDirection == Vector2.zero)
    {
      if (isOutside) OnEnterConfinement.Invoke();

      isOutside = false;

      return;
    }

    if (isOutside == false) OnLeaveConfinement.Invoke();

    isOutside = true;

    OnOutsideConfinement.Invoke(areaDirection);
  }
}

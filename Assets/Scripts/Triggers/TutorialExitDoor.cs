using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialExitDoor : Triggerable
{
  protected override IEnumerator TriggerAction()
  {
    BoxCollider2D ownCollider = GetComponent<BoxCollider2D>();
    Movement movement = GetComponent<Movement>();

    // Get target position
    Vector2 targetPosition = (Vector2)transform.position + Vector2.up * ownCollider.bounds.extents.y * 2;

    // Move towards it
    yield return movement.MoveTo(targetPosition);
  }
}

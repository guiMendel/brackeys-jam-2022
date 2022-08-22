using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class NpcController : MonoBehaviour
{

  // === INTERFACE

  [Tooltip("How likely it is for the NPC to stand idle instead of moving in a given movement cycle")]
  public float idleChance = 0.3f;

  [Tooltip("The min and max duration of each movement cycle")]
  public Vector2 movementCycleDurationLimits = new Vector2(0.2f, 2f);


  // === STATE

  // The coroutine that keeps changing it's movement
  Coroutine movementCoroutine;


  // === REFS

  Movement movement;

  private void Awake()
  {
    movement = GetComponent<Movement>();

    EnsureNotNull.Objects(movement);
  }

  private void Start()
  {
    // Start the movement cycles
    movementCoroutine = StartCoroutine(CountCycles());
  }

  private void OnTriggerExit2D(Collider2D other)
  {
    if (movementCoroutine != null) StopCoroutine(movementCoroutine);

    // Get angle to area center
    float collisionAngle = Mathf.Atan2(other.bounds.center.y - transform.position.y, other.bounds.center.x - transform.position.x);

    // Set starting angle to move towards it
    movementCoroutine = StartCoroutine(CountCycles(collisionAngle, noInitialIdle: true));
  }

  private void RestartCycleToAngle(float angle)
  {
    if (movementCoroutine != null) StopCoroutine(movementCoroutine);

    // Set starting angle to the inverse of the collision angle
    movementCoroutine = StartCoroutine(CountCycles(angle, noInitialIdle: true));
  }

  private void StartNewCycle(float angle, bool idle)
  {
    Vector2 movementVector = idle
      ? Vector2.zero
      : new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

    movement.SetTargetMovement(movementVector);
  }

  private IEnumerator CountCycles()
  {
    return CountCycles(Random.Range(0f, 2 * Mathf.PI));
  }

  private IEnumerator CountCycles(float startingAngle, bool noInitialIdle = false)
  {
    float angle = startingAngle;
    bool idle = noInitialIdle ? false : Random.value <= idleChance;

    // Loop forever
    while (true)
    {
      // Start new cycle
      StartNewCycle(angle, idle);

      // Get it's duration
      float duration = Gaussian.Random(movementCycleDurationLimits.x, movementCycleDurationLimits.y);

      // Wait the given time
      yield return new WaitForSeconds(duration);

      // Get idle chance
      idle = Random.value <= idleChance;

      // Get new angle
      if (!idle) angle = Random.Range(0f, 2 * Mathf.PI);
    }
  }
}

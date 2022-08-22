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

  [Tooltip("Area to confine it's movement to")]
  public BoxCollider2D movementArea;


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

  private void Update()
  {
    if (transform.position.x < movementArea.bounds.min.x) RestartCycleToAngle(0);

    else if (transform.position.x > movementArea.bounds.max.x) RestartCycleToAngle(Mathf.PI);

    else if (transform.position.y > movementArea.bounds.max.y) RestartCycleToAngle(Mathf.PI * 1.5f);

    else if (transform.position.y < movementArea.bounds.min.y) RestartCycleToAngle(Mathf.PI * 0.5f);
  }

  // private void OnTriggerExit2D(Collider2D other)
  // {
  //   if (movementCoroutine != null) StopCoroutine(movementCoroutine);

  //   // Get angle to area center
  //   float centerAngle = Mathf.Atan2(other.bounds.center.y - transform.position.y, other.bounds.center.x - transform.position.x);

  //   // Add some variation to it
  //   centerAngle += Random.Range(-Mathf.PI / 4, Mathf.PI / 4);

  //   // Set starting angle to move towards it
  //   movementCoroutine = StartCoroutine(CountCycles(centerAngle, noInitialIdle: true));
  // }

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

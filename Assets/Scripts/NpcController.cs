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
  AreaConfine confine;

  private void Awake()
  {
    movement = GetComponent<Movement>();
    confine = GetComponent<AreaConfine>();

    EnsureNotNull.Objects(movement, confine);
  }

  private void OnEnable()
  {
    // Start the movement cycles
    movementCoroutine = StartCoroutine(CountCycles());

    // Subscribe to area confinement
    confine.OnLeaveConfinement.AddListener(ReturnToArea);
  }

  private void OnDisable()
  {
    if (movementCoroutine != null)
    {
      StopCoroutine(movementCoroutine);
      movementCoroutine = null;
    }
    confine.OnLeaveConfinement.RemoveListener(ReturnToArea);
  }

  private void ReturnToArea(Vector2 areaDirection)
  {
    if (movementCoroutine != null)
    {
      StopCoroutine(movementCoroutine);
      movementCoroutine = null;
    }

    // Set starting angle to the inverse of the collision angle
    movementCoroutine = StartCoroutine(CountCycles(areaDirection, noInitialIdle: true));
  }

  private void StartNewCycle(Vector2 direction, bool idle)
  {
    movement.SetTargetMovement(idle ? Vector2.zero : direction);
  }

  private IEnumerator CountCycles()
  {
    return CountCycles(Vector2.right.RandomRotated());
  }

  private IEnumerator CountCycles(Vector2 startingDirection, bool noInitialIdle = false)
  {
    Vector2 direction = startingDirection;
    bool idle = noInitialIdle ? false : Random.value <= idleChance;

    // Loop forever
    while (true)
    {
      // Start new cycle
      StartNewCycle(direction, idle);

      // Get it's duration
      float duration = Gaussian.Random(movementCycleDurationLimits.x, movementCycleDurationLimits.y);

      // Wait the given time
      yield return new WaitForSeconds(duration);

      // Get idle chance
      idle = Random.value <= idleChance;

      // Get new angle
      if (!idle) direction = direction.RandomRotated();
    }
  }
}

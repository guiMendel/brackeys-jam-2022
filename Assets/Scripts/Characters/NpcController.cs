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

  // Coroutine to turn into alien
  Coroutine turnCoroutine;

  public Vector2 initialPosition;


  // === REFS

  Movement movement;
  AreaConfine confine;


  // === INTERFACE

  public void CancelTurn()
  {
    if (turnCoroutine == null) return;
    StopCoroutine(turnCoroutine);
    turnCoroutine = null;
  }

  public void TurnIntoAlien(float delay = 0)
  {
    CancelTurn();

    turnCoroutine = StartCoroutine(TurnIntoAlienCoroutine(delay));
  }

  private IEnumerator TurnIntoAlienCoroutine(float delay = 0)
  {
    yield return new WaitForSeconds(delay);

    // Stop if is in move animation
    if (GetComponent<SpawnAnimationScript>() != null) yield break;

    AlienTargetManager alienTargetManager = FindObjectOfType<AlienTargetManager>();

    // Create alien in this position
    AlienController newAlien = alienTargetManager.CreateAlienAt(transform);

    // Store the npc skin
    newAlien.NpcSkin = GetComponent<Skin>().ActiveSkin;

    // Store it's confine area
    newAlien.NpcConfine = confine.movementArea;
    newAlien.NpcPosition = initialPosition;
    newAlien.NpcIdleChance = idleChance;

    // Apply modifier
    newAlien.maxOffset = newAlien.maxOffset * alienTargetManager.angleOffsetModifier;

    newAlien.tag = "Alien";

    alienTargetManager.SwitchAlienObject(gameObject, newAlien.gameObject);

    turnCoroutine = null;

    // Remove the npc
    gameObject.SetActive(false);
    Destroy(gameObject);
  }

  private void Awake()
  {
    movement = GetComponent<Movement>();
    confine = GetComponent<AreaConfine>();

    EnsureNotNull.Objects(movement);
  }

  private void OnEnable()
  {
    initialPosition = transform.position;
    
    // Start the movement cycles
    StartRandomWalk();

    // Subscribe to area confinement
    confine?.OnOutsideConfinement.AddListener(ReturnToArea);
    confine?.OnEnterConfinement.AddListener(StartRandomWalk);
  }

  private void OnDisable()
  {
    if (movementCoroutine != null)
    {
      StopCoroutine(movementCoroutine);
      movementCoroutine = null;
    }
    confine?.OnOutsideConfinement.RemoveListener(ReturnToArea);
    confine?.OnEnterConfinement.RemoveListener(StartRandomWalk);
  }

  private void StartRandomWalk()
  {
    if (movementCoroutine != null) StopCoroutine(movementCoroutine);

    movementCoroutine = StartCoroutine(CountCycles());
  }

  private void ReturnToArea(Vector2 areaDistance)
  {
    if (movementCoroutine != null)
    {
      StopCoroutine(movementCoroutine);
      movementCoroutine = null;
    }

    // Set starting angle to the inverse of the collision angle
    movement.MoveTo(confine.movementArea.bounds.center);
  }

  private void StartNewCycle(Vector2 direction, bool idle)
  {
    if (idle && movement.Destination != null) return;
    
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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackedLifeRepeater : MonoBehaviour
{
  // === PROPERTY

  public PlayerLifeTracker.LifeEntry LifeEntry { get; private set; }


  // === STATE

  // Next input entry to execute
  PlayerLifeTracker.InputEntry? nextInputEntry = null;

  // Whether already triggered aggro
  bool aggroTriggered = false;


  // === REFS

  Movement movement;
  NpcManager npcManager;
  AlienTargetManager alienTargetManager;

  public void SetLifeEntry(PlayerLifeTracker.LifeEntry lifeEntry)
  {
    LifeEntry = PlayerLifeTracker.LifeEntry.Copy(lifeEntry);
  }


  private void Awake()
  {
    movement = GetComponent<Movement>();
    npcManager = FindObjectOfType<NpcManager>();
    alienTargetManager = FindObjectOfType<AlienTargetManager>();

    EnsureNotNull.Objects(movement, npcManager, alienTargetManager);
  }

  private void Start() { AdvanceInputQueue(); }

  private void Update()
  {
    ExecuteInputs();

    DetectAggroStart();

    DetectDeathTime();
  }

  private void DetectDeathTime()
  {
    if (Time.timeSinceLevelLoad < LifeEntry.deathTime) return;


    // Place a regular npc in place of this one
    alienTargetManager.SwitchTarget(gameObject, npcManager.CreateNpcAt(transform.position));

    gameObject.SetActive(false);
    Destroy(gameObject);
  }

  private void DetectAggroStart()
  {
    if (Time.timeSinceLevelLoad < LifeEntry.aggroTime || aggroTriggered) return;

    aggroTriggered = true;

    // Set aggro
    alienTargetManager.AddTarget(gameObject);
  }

  private void ExecuteInputs()
  {
    // Check if it's time to execute this entry
    if (nextInputEntry != null && Time.timeSinceLevelLoad >= nextInputEntry.Value.timeStamp)
    {
      // Execute the input entry
      movement.SetTargetMovement(nextInputEntry.Value.movementDirection);

      // Advance queue
      AdvanceInputQueue();
    }
  }

  private void AdvanceInputQueue()
  {
    nextInputEntry = LifeEntry.inputEntries.Count > 0
      ? LifeEntry.inputEntries.Dequeue()
      : null;
  }
}

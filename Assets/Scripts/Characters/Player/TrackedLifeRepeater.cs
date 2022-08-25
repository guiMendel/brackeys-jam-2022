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
  PlayerLifeTracker.InputEntry nextInputEntry = null;

  // Whether already triggered aggro
  bool aggroTriggered = false;


  // === REFS

  PlayerCharacter playerCharacter;
  NpcManager npcManager;
  AlienTargetManager alienTargetManager;

  public void SetLifeEntry(PlayerLifeTracker.LifeEntry lifeEntry)
  {
    LifeEntry = PlayerLifeTracker.LifeEntry.Copy(lifeEntry);
  }


  private void Awake()
  {
    playerCharacter = GetComponent<PlayerCharacter>();
    npcManager = FindObjectOfType<NpcManager>();
    alienTargetManager = FindObjectOfType<AlienTargetManager>();

    EnsureNotNull.Objects(playerCharacter, npcManager, alienTargetManager);
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
    if (LifeEntry.aggroTime > 0f && (Time.timeSinceLevelLoad < LifeEntry.aggroTime || aggroTriggered)) return;

    aggroTriggered = true;

    // Set aggro
    alienTargetManager.AddTarget(gameObject);
  }

  private void ExecuteInputs()
  {
    // Check if it's time to execute this entry
    if (nextInputEntry != null && Time.timeSinceLevelLoad >= nextInputEntry.timeStamp)
    {
      // Execute the input entry
      if (nextInputEntry is PlayerLifeTracker.MovementInput)
      {
        playerCharacter.Move((nextInputEntry as PlayerLifeTracker.MovementInput).direction);
      }

      else
      {
        playerCharacter.Sprint((nextInputEntry as PlayerLifeTracker.SprintInput).toggle);
      }

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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SuspicionMeter : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How much suspicion lowers each second")]
  public float suspicionDecay = 5f;

  [Tooltip("How much suspicion each unit of distance from confine are accrues per second")]
  public float confinementBreakSuspicion = 20f;

  [Tooltip("How long player can be idle before raising suspicion")]
  public float idleSlackTime = 4f;

  [Tooltip("How much suspicion raise speed is accrued for each second of being idle")]
  public float idleSuspicion = 2f;

  [Tooltip("How much suspicion is raised each second the player is sprinting")]
  public float sprintSuspicion = 10f;

  [Tooltip("The Spotlight to turn on on player aggro")]
  public GameObject spotlight;

  [Tooltip("The lamps to turn off on player aggro")]
  public GameObject[] lamps;

  [Tooltip("Raised when the aliens aggro to the player character (not the player trackers)")]
  public UnityEvent OnAggro;


  // === STATE

  // Whether already triggered
  public bool Triggered { get; private set; } = false;

  // How much time has passed since player's las input
  float timeSinceLastInput = 0f;

  // Suspicion raise from area confinement
  float confineSuspicionRaise = 0f;

  // Suspicion raise from being idle
  float idleSuspicionRaise = 0f;


  // === PROPERTIES

  // How suspicious the player is. Ranges from 0 to 100
  public float SuspicionLevel { get; private set; } = 0f;

  // Speed at which the suspicion is raising this frame
  public float SuspicionRaise { get; private set; } = 0f;

  bool PlayerSprinting => playerController != null
    && playerController.GetComponent<PlayerCharacter>().IsSprinting
    && playerController.GetComponent<Movement>().MovementDirection != Vector2.zero;


  // === REFS

  AreaConfine playerConfine;
  PlayerController playerController;


  private void Awake()
  {
    OnAggro ??= new UnityEvent();

    playerController = FindObjectOfType<PlayerController>();
    playerConfine = playerController.GetComponent<AreaConfine>();

    EnsureNotNull.Objects(playerConfine, playerController);
  }

  private void Update()
  {
    // Detect idle
    idleSuspicionRaise += timeSinceLastInput <= idleSlackTime
      ? 0f
      : idleSuspicion * (timeSinceLastInput - idleSlackTime);

    SuspicionRaise = confineSuspicionRaise + idleSuspicionRaise;

    // Detect sprint
    if (PlayerSprinting) SuspicionRaise += sprintSuspicion;

    // Raise suspicion
    if (SuspicionRaise != 0f)
    {
      SuspicionLevel += SuspicionRaise * Time.deltaTime;

      // Check for aggro trigger
      if (SuspicionLevel >= 100f) TriggerAggro();
    }

    // Slow suspicion decay
    else SuspicionLevel -= suspicionDecay * Time.deltaTime;

    // Clamp it
    SuspicionLevel = Mathf.Clamp(SuspicionLevel, 0f, 100f);

    // Raise idle counter
    timeSinceLastInput += Time.deltaTime;
  }

  private void OnEnable()
  {
    playerConfine.OnOutsideConfinement.AddListener(RaiseConfineSuspicion);
    playerConfine.OnEnterConfinement.AddListener(StopConfineRaising);
    playerController.OnPlayerMove.AddListener(TrackInput);

  }

  private void OnDisable()
  {
    playerConfine?.OnOutsideConfinement.RemoveListener(RaiseConfineSuspicion);
    playerConfine?.OnEnterConfinement.RemoveListener(StopConfineRaising);
    playerController?.OnPlayerMove.RemoveListener(TrackInput);
  }

  private void TrackInput(Vector2 direction)
  {
    timeSinceLastInput = 0f;
    idleSuspicionRaise = 0f;
  }

  private void StopConfineRaising()
  {
    confineSuspicionRaise = 0f;
  }

  private void RaiseConfineSuspicion(Vector2 areaDistance)
  {
    confineSuspicionRaise = areaDistance.magnitude * confinementBreakSuspicion;
  }

  private void TriggerAggro()
  {
    if (Triggered || playerController == null) return;

    Triggered = true;

    OnAggro.Invoke();

    // No more decay
    suspicionDecay = 0f;

    // Add player as target
    FindObjectOfType<AlienTargetManager>().AddTarget(playerController.gameObject);

    // Turn on spotlight
    TurnOnSpotlight();
  }

  private void TurnOnSpotlight()
  {
    foreach (GameObject lamp in lamps) lamp.SetActive(false);
    spotlight.SetActive(true);
  }
}

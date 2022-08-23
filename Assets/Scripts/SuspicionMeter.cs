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


  // === REFS

  AreaConfine playerConfine;


  private void Awake()
  {
    OnAggro ??= new UnityEvent();

    playerConfine = FindObjectOfType<PlayerController>()
      .GetComponent<AreaConfine>();

    EnsureNotNull.Objects(playerConfine);
  }

  private void Update()
  {
    // Detect idle
    idleSuspicionRaise += timeSinceLastInput <= idleSlackTime
      ? 0f
      : idleSuspicion * (timeSinceLastInput - idleSlackTime);

    SuspicionRaise = confineSuspicionRaise + idleSuspicionRaise;

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
    FindObjectOfType<PlayerController>().OnPlayerInput.AddListener(TrackInput);

  }

  private void OnDisable()
  {
    playerConfine.OnOutsideConfinement.RemoveListener(RaiseConfineSuspicion);
    playerConfine.OnEnterConfinement.RemoveListener(StopConfineRaising);
    FindObjectOfType<PlayerController>()?.OnPlayerInput.RemoveListener(TrackInput);
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
    if (Triggered) return;

    Triggered = true;

    OnAggro.Invoke();

    // No more decay
    suspicionDecay = 0f;

    // Add player as target
    FindObjectOfType<AlienTargetManager>().AddTarget(FindObjectOfType<PlayerController>().gameObject);
  }
}

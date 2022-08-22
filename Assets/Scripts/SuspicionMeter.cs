using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SuspicionMeter : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How much suspicion each unit of distance from confine are accrues per second")]
  public float confinementBreakSuspicion = 20f;

  [Tooltip("How much suspicion lowers each second")]
  public float suspicionDecay = 5f;


  // === STATE


  // === PROPERTIES

  // How suspicious the player is. Ranges from 0 to 100
  public float SuspicionLevel { get; private set; } = 0f;

  // Speed at which the suspicion is raising this frame
  public float SuspicionRaise { get; private set; } = 0f;


  // === REFS

  static SuspicionMeter instance;
  AreaConfine playerConfine;


  private void Awake()
  {
    playerConfine = FindObjectOfType<PlayerController>()
      .GetComponent<AreaConfine>();

    EnsureNotNull.Objects(playerConfine);
  }

  private void Update()
  {
    // Raise suspicion
    if (SuspicionRaise != 0f)
    {
      SuspicionLevel += SuspicionRaise * Time.deltaTime;
    }

    // Slow suspicion decay
    else SuspicionLevel -= suspicionDecay * Time.deltaTime;

    // Clamp it
    SuspicionLevel = Mathf.Clamp(SuspicionLevel, 0f, 100f);
  }

  private void SingletonCheck()
  {
    if (instance != null && instance != this)
    {
      // If there's already an instance, stop
      gameObject.SetActive(false);
      Destroy(gameObject);

      return;
    }

    instance = this;
  }

  private void OnEnable()
  {
    SingletonCheck();
    playerConfine.OnOutsideConfinement.AddListener(RaiseSuspicion);
    playerConfine.OnEnterConfinement.AddListener(StopRaising);
  }

  private void OnDisable()
  {
    playerConfine.OnOutsideConfinement.RemoveListener(RaiseSuspicion);
    playerConfine.OnEnterConfinement.RemoveListener(StopRaising);
  }

  private void StopRaising()
  {
    SuspicionRaise = 0f;
  }

  private void RaiseSuspicion(Vector2 areaDistance)
  {
    SuspicionRaise = areaDistance.magnitude * confinementBreakSuspicion;
  }
}

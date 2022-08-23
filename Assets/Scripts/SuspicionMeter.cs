using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class SuspicionMeter : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How much suspicion each unit of distance from confine are accrues per second")]
  public float confinementBreakSuspicion = 20f;

  [Tooltip("How much suspicion lowers each second")]
  public float suspicionDecay = 5f;

  public UnityEvent OnAggro;


  // === STATE

  // Whether already triggered
  public bool Triggered { get; private set; } = false;


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
  }

  private void OnEnable()
  {
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

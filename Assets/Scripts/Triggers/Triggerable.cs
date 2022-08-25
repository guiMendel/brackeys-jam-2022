using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Triggerable : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Whether it can trigger more than once")]
  public bool triggerMultipleTimes = false;


  // === STATE

  public bool Triggered { get; private set; } = false;


  public void Trigger()
  {
    if (Triggered && triggerMultipleTimes == false) return;

    Triggered = true;

    StartCoroutine(TriggerAction());
  }

  protected abstract IEnumerator TriggerAction();
}

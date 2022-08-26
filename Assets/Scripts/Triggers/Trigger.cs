using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Targets to activate on trigger")]
  public List<Triggerable> targets;

  [Tooltip("Whether it only triggers once")]
  public bool oneShot = false;

  [Tooltip("Whether it's player controlled only")]
  public bool playerControlledOnly;

  public UnityEvent OnTrigger;


  // === STATE

  bool triggered = false;


  private void OnTriggerEnter2D(Collider2D other)
  {
    if (
      oneShot && triggered
      || playerControlledOnly && other.GetComponent<PlayerController>() == null
    ) return;
    triggered = true;

    foreach (var target in targets) target.Trigger();

    OnTrigger.Invoke();
  }
}

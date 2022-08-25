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

  public UnityEvent OnTrigger;


  private void OnTriggerEnter2D(Collider2D other)
  {
    foreach (var target in targets) target.Trigger();

    OnTrigger.Invoke();
  }
}

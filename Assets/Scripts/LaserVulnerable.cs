using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserVulnerable : MonoBehaviour
{
  // === PARAMS

  public GameObject destroyTarget;

  public UnityEvent OnDeath;


  private void Awake()
  {
    OnDeath ??= new UnityEvent();

    EnsureNotNull.Objects(OnDeath, destroyTarget);
  }


  public void Die()
  {
    OnDeath.Invoke();

    Destroy(destroyTarget);
  }
}

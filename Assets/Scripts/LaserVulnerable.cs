using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserVulnerable : MonoBehaviour
{
  // === PARAMS

  public UnityEvent OnDeath;


  private void Awake()
  {
    OnDeath ??= new UnityEvent();

    EnsureNotNull.Objects(OnDeath);
  }


  public void Die()
  {
    OnDeath.Invoke();

    Destroy(gameObject);
  }
}

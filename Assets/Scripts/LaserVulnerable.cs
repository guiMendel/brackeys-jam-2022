using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LaserVulnerable : MonoBehaviour
{
  // === PARAMS

  public GameObject destroyTarget;

  public UnityEvent OnDeath;

  public bool godMode = false;


  // === STATE

  public bool Dead { get; private set; } = false;


  private void Awake()
  {
    OnDeath ??= new UnityEvent();

    EnsureNotNull.Objects(OnDeath, destroyTarget);
  }


  public void Die()
  {
    if (Dead || godMode) return;

    Dead = true;

    OnDeath.Invoke();

    destroyTarget.SetActive(false);
    Destroy(destroyTarget);
  }
}

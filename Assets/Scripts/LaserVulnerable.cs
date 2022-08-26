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

  public float destroyDelay = 0f;


  // === STATE

  public bool Dead { get; private set; } = false;


  private void Awake()
  {
    OnDeath ??= new UnityEvent();

    EnsureNotNull.Objects(OnDeath);
  }


  public void Die()
  {
    if (Dead || godMode) return;

    Dead = true;

    OnDeath.Invoke();

    GameObject target = destroyTarget == null ? gameObject : destroyTarget;

    StartCoroutine(DestroyDelayed(target));
  }

  private IEnumerator DestroyDelayed(GameObject target)
  {
    if (destroyDelay > 0f) yield return new WaitForSeconds(destroyDelay);

    target.SetActive(false);
    Destroy(target);
  }
}

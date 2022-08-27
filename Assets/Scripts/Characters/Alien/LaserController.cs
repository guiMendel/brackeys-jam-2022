using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

public class LaserController : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How many degrees per second the laser is able to adjust to follow it's target")]
  [Range(0f, 180f)] public float homingPotency = 10f;

  [Tooltip("How fast the laser goes")]
  [Min(0f)] public float speed = 5f;

  public UnityEvent OnImpact;


  // === PROPERTIES

  // Object that owns this laser beam
  public GameObject Owner { get; set; }

  public GameObject Target { get; private set; }

  Vector2 PredictedTargetDirection =>
    MovementPrediction.PredictTargetMovement(transform, Target.transform, speed);


  // === REFS

  Rigidbody2D body;


  private void Awake()
  {
    body = GetComponent<Rigidbody2D>();

    EnsureNotNull.Objects(body);
  }


  public void SetTarget(GameObject target)
  {
    Target = target;

    // Predict target position
    body.velocity = PredictedTargetDirection * speed;
  }

  private Vector3 PredictTargetPosition()
  {
    return Target.transform.position;
  }

  private void Update()
  {
    if (Target == null) return;

    // Homing
    float angleDelta = Mathf.Clamp(
      Vector2.SignedAngle(body.velocity, PredictedTargetDirection),
      -homingPotency,
      homingPotency
    ) * Time.deltaTime;

    body.velocity = body.velocity.Rotated(angleDelta);
  }

  private void OnTriggerEnter2D(Collider2D other)
  {
    OnImpact.Invoke();

    PlayParticles();

    if (other.gameObject.CompareTag("StopLaser"))
    {
      Stop();
      return;
    }

    LaserVulnerable hitVulnerable = other.GetComponent<LaserVulnerable>();

    // Ignore if owner
    if (hitVulnerable == null || GameObject.ReferenceEquals(Owner, hitVulnerable.destroyTarget)) return;

    // Try to kill it
    hitVulnerable.Die();
  }

  private void PlayParticles()
  {
    foreach (var system in GetComponentsInChildren<ParticleSystem>())
    {
      if (GameObject.ReferenceEquals(system.gameObject, gameObject)) continue;
      system.Emit(Mathf.RoundToInt(system.emission.rateOverTime.constant * system.main.duration));
    }
  }

  private void Stop()
  {
    Target = null;
    Destroy(body);
    GetComponent<Collider2D>().enabled = false;

    GetComponentInChildren<ParticleSystem>().Stop();
    GetComponent<Light2D>().enabled = false;

    Destroy(gameObject, 5f);
  }
}

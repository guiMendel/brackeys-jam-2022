using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LaserController : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How many degrees per second the laser is able to adjust to follow it's target")]
  [Range(0f, 180f)] public float homingPotency = 10f;

  [Tooltip("How fast the laser goes")]
  [Min(0f)] public float speed = 5f;


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
    if (other.gameObject.CompareTag("Bounds"))
    {
      Stop();
      return;
    }

    // Ignore if owner
    if (GameObject.ReferenceEquals(Owner, other.gameObject)) return;

    // Try to kill it
    other.GetComponent<LaserVulnerable>()?.Die();
  }

  private void Stop()
  {
    Target = null;
    Destroy(body);
    GetComponent<Collider2D>().enabled = false;

    GetComponentInChildren<ParticleSystem>().Stop();

    Destroy(gameObject, 5f);
  }
}

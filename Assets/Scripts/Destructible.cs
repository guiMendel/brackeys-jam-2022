using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destructible : MonoBehaviour
{
  // === REFS

  LaserVulnerable vulnerable;
  ParticleSystem destroyParticles;
  Collider2D ownCollider;
  SpriteRenderer[] spriteRenderers;

  private void Awake()
  {
    vulnerable = GetComponent<LaserVulnerable>();
    destroyParticles = GetComponent<ParticleSystem>();
    spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    ownCollider = GetComponent<Collider2D>();

    EnsureNotNull.Objects(vulnerable, destroyParticles);
  }

  private void OnEnable()
  {
    vulnerable.OnDeath.AddListener(PlayParticles);
  }

  private void OnDisable()
  {
    vulnerable.OnDeath.RemoveListener(PlayParticles);
  }

  private void PlayParticles()
  {
    foreach (var renderer in spriteRenderers) Destroy(renderer);
    Destroy(ownCollider);

    destroyParticles.Play();
  }
}

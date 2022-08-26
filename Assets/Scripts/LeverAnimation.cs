using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeverAnimation : Triggerable
{
  // === PARAMS

  [Tooltip("Sprite to use when toggled")]
  public Sprite toggledSprite;


  // === REFS

  ParticleSystem sparks;
  SpriteRenderer spriteRenderer;

  private void Awake()
  {
    sparks = GetComponent<ParticleSystem>();
    spriteRenderer = GetComponent<SpriteRenderer>();

    EnsureNotNull.Objects(toggledSprite, sparks, spriteRenderer);
  }

  protected override IEnumerator TriggerAction()
  {
    sparks.Play();
    spriteRenderer.sprite = toggledSprite;

    yield break;
  }

}

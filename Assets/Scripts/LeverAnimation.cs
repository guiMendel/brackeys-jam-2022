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
  AudioPlayer audioPlayer;

  private void Awake()
  {
    sparks = GetComponent<ParticleSystem>();
    spriteRenderer = GetComponent<SpriteRenderer>();
    audioPlayer = GetComponent<AudioPlayer>();

    EnsureNotNull.Objects(toggledSprite, sparks, spriteRenderer, audioPlayer);
  }

  protected override IEnumerator TriggerAction()
  {
    sparks.Play();
    spriteRenderer.sprite = toggledSprite;
    audioPlayer.Play();

    yield break;
  }

}

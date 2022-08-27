using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class PlaySound : Triggerable
{
  // === PARAMS

  public AudioClip clip;
  public float volumeScale;
  public bool loop = false;


  // === STATE

  AudioSource source;

  private void Awake()
  {
    source = gameObject.AddComponent<AudioSource>();
  }


  protected override IEnumerator TriggerAction()
  {
    if (loop)
    {
      source.loop = true;
      source.clip = clip;
      source.Play();
    }

    else source.PlayOneShot(clip, volumeScale);

    yield break;
  }
}

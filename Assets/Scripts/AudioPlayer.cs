using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
  // === PARAMS

  public string title;
  public AudioClip[] clips;
  [Range(0f, 1f)] public float volumeScale = 1f;
  public bool loop = false;
  public bool playOnAwake = false;


  // === STATE

  AudioSource Source
  {
    get
    {
      AudioSource source = GetComponent<AudioSource>();
      return source != null ? source : gameObject.AddComponent<AudioSource>();
    }
  }

  private void Awake()
  {
    if (playOnAwake) Play();
  }

  public void Play() { Play(-1); }

  public void Play(int index)
  {
    if (index == -1) index = Random.Range(0, clips.Length);

    AudioClip clip = clips[index];

    if (loop)
    {
      Source.loop = true;
      Source.clip = clip;
      Source.Play();
    }

    else AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, volumeScale);
  }
}

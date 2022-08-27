using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour
{
  // === PARAMS

  public SpriteRenderer doorRenderer;

  public float duration = 0.5f;


  // === STATE

  Coroutine closeCoroutine;


  // === REFS

  AudioPlayer audioPlayer;


  private void Awake()
  {
    audioPlayer = GetComponent<AudioPlayer>();
  }


  public void Open()
  {
    // Open sound
    audioPlayer.Play(0);

    doorRenderer.enabled = true;

    if (duration <= 0f) return;

    if (closeCoroutine != null) StopCoroutine(closeCoroutine);

    closeCoroutine = StartCoroutine(CloseAfterDelay());
  }

  IEnumerator CloseAfterDelay()
  {
    yield return new WaitForSeconds(duration);

    // Close sound
    audioPlayer.Play(1);

    doorRenderer.enabled = false;

    closeCoroutine = null;
  }
}

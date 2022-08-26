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


  public void Open()
  {
    doorRenderer.enabled = true;

    if (duration <= 0f) return;

    if (closeCoroutine != null) StopCoroutine(closeCoroutine);

    closeCoroutine = StartCoroutine(CloseAfterDelay());
  }

  IEnumerator CloseAfterDelay()
  {
    yield return new WaitForSeconds(duration);

    doorRenderer.enabled = false;

    closeCoroutine = null;
  }
}

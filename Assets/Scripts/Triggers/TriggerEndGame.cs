using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerEndGame : MonoBehaviour
{
  private void OnEnable()
  {
    GetComponent<Trigger>().OnTrigger.AddListener(EndGame);
  }

  private void OnDisable()
  {
    GetComponent<Trigger>()?.OnTrigger.RemoveListener(EndGame);
  }

  private void EndGame()
  {
    FindObjectOfType<SceneHandler>().LoadNextScene("EndGame");
  }
}

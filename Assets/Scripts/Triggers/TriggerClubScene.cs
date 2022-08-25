using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerClubScene : MonoBehaviour
{
  private void OnEnable()
  {
    GetComponent<Trigger>().OnTrigger.AddListener(GoToClub);
  }

  private void OnDisable()
  {
    GetComponent<Trigger>()?.OnTrigger.RemoveListener(GoToClub);
  }

  private void GoToClub()
  {
    FindObjectOfType<SceneHandler>().LoadNextScene("Club");
  }
}

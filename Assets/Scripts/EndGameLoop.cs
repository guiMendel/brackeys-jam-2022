using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameLoop : MonoBehaviour
{
  private void OnEnable()
  {
    FindObjectOfType<DialogueHandler>().OnFinish.AddListener(() =>
    {
      SceneManager.LoadScene("Tutorial");
    });
  }
}

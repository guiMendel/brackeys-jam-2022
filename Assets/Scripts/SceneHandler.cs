using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
  // === REFS

  static SceneHandler instance;


  private void SingletonCheck()
  {
    if (instance != null && instance != this)
    {
      // If there's already an instance, stop
      gameObject.SetActive(false);
      Destroy(gameObject);

      return;
    }

    instance = this;
    DontDestroyOnLoad(gameObject);
  }

  private void Start()
  {
    SingletonCheck();
  }

  public void ReloadScene(float delay = 0f)
  {
    StartCoroutine(RestartSceneIn(delay));
  }

  IEnumerator RestartSceneIn(float seconds)
  {
    yield return new WaitForSeconds(seconds);

    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
}
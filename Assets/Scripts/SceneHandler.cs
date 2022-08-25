using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
  // === PARAMS

  public float preRestartCurtainCloseTime = 1f;


  // === STATE

  bool checkpointEnabled = false;
  Bounds checkpointSpawnPosition;
  Vector2 checkpointSpawnPositionCamera;


  // === REFS

  static SceneHandler instance;


  // === INTERFACE

  public void SetSpawnPoints(Bounds playerPoint, Vector2 cameraPoint)
  {
    checkpointSpawnPosition = playerPoint;
    checkpointSpawnPositionCamera = cameraPoint;
    checkpointEnabled = true;
    UseCheckpoint();
  }

  public void ReloadScene(float delay = 0f)
  {
    StartCoroutine(RestartSceneIn(delay));
  }


  private void SingletonCheck()
  {
    if (instance != null && instance != this)
    {
      // Pass params ahead
      instance.preRestartCurtainCloseTime = preRestartCurtainCloseTime;

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

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
  {
    UseCheckpoint();
  }

  private void UseCheckpoint()
  {
    if (checkpointEnabled)
    {
      print(("setting it up", checkpointSpawnPosition));
      FindObjectOfType<PlayerController>().SpawnArea = checkpointSpawnPosition;
      Camera.main.transform.position = new Vector3(
        checkpointSpawnPositionCamera.x, checkpointSpawnPositionCamera.y, Camera.main.transform.position.z
      );
    }
  }

  IEnumerator RestartSceneIn(float seconds)
  {
    yield return new WaitForSeconds(seconds - preRestartCurtainCloseTime);

    FindObjectOfType<UICurtain>().Close();

    yield return new WaitForSeconds(preRestartCurtainCloseTime);

    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
  }
}

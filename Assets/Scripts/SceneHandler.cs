using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneHandler : MonoBehaviour
{
  // === PARAMS

  public float preRestartCurtainCloseTime = 1f;

  public InputAction RestartSceneAction;

  public AudioClip themeOriginal;

  public float aggroMusicPitch = 1.5f;


  // === STATE

  bool checkpointEnabled = false;
  Bounds checkpointSpawnPosition;
  Vector2 checkpointSpawnPositionCamera;
  bool ignoreReload = false;
  bool restarting = false;


  // === REFS

  static SceneHandler instance;
  AudioSource musicSource;


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
    if (ignoreReload) return;

    StartCoroutine(LoadSceneIn(SceneManager.GetActiveScene().name, delay));
  }

  public void Restart(InputAction.CallbackContext context)
  {
    if (context.performed == false || restarting) return;
    restarting = true;

    ResetStates(lifeTracksOnly: true);
    ReloadScene();
  }

  public void LoadNextScene(string sceneName)
  {
    ResetStates();

    StartCoroutine(LoadSceneIn(sceneName, 1f));
  }


  private void ResetStates(bool lifeTracksOnly = false)
  {
    FindObjectOfType<PlayerLifeTracker>().EraseEntries();
    if (lifeTracksOnly) return;
    FindObjectOfType<DialogueHandler>().ResetDialogues();
    checkpointEnabled = false;
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

  private void Awake()
  {
    musicSource = GetComponent<AudioSource>();
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
    RestartSceneAction.performed += Restart;
    RestartSceneAction.Enable();
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
    RestartSceneAction.performed -= Restart;
    RestartSceneAction.Disable();
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
  {
    if (scene.name == "EndGame")
    {
      gameObject.SetActive(false);
      Destroy(gameObject);
      return;
    }

    musicSource.pitch = 1f;

    FindObjectOfType<SuspicionMeter>().OnAggro.AddListener(
      () => musicSource.pitch = aggroMusicPitch
    );

    if (scene.name != "Tutorial")
    {
      int time = musicSource.timeSamples;

      musicSource.clip = themeOriginal;

      musicSource.timeSamples = time;
      musicSource.Play();
    }

    UseCheckpoint();
    restarting = false;
  }

  private void UseCheckpoint()
  {
    if (checkpointEnabled)
    {
      FindObjectOfType<PlayerController>().SpawnArea = checkpointSpawnPosition;
      Camera.main.transform.position = new Vector3(
        checkpointSpawnPositionCamera.x, checkpointSpawnPositionCamera.y, Camera.main.transform.position.z
      );
    }
  }

  IEnumerator LoadSceneIn(string sceneName, float seconds)
  {
    ignoreReload = true;

    if (seconds > preRestartCurtainCloseTime)
      yield return new WaitForSeconds(seconds - preRestartCurtainCloseTime);

    FindObjectOfType<UICurtain>().Close();

    yield return new WaitForSeconds(preRestartCurtainCloseTime);

    SceneManager.LoadScene(sceneName);

    yield return new WaitForEndOfFrame();

    ignoreReload = false;
  }
}

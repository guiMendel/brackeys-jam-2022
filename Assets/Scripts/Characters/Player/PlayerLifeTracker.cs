using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerLifeTracker : MonoBehaviour
{
  // Defines an input entry
  public class InputEntry
  {
    // When this input happened
    public float timeStamp;

    public InputEntry(float timeStamp)
    {
      this.timeStamp = timeStamp;
    }
  }

  public class MovementInput : InputEntry
  {
    // Direction
    public Vector2 direction;

    public MovementInput(float timeStamp, Vector2 direction) : base(timeStamp)
    {
      this.direction = direction;
    }
  }

  public class SprintInput : InputEntry
  {
    // Whether to toggle it on or off
    public bool toggle;

    public SprintInput(float timeStamp, bool toggle) : base(timeStamp)
    {
      this.toggle = toggle;
    }
  }

  // Defines a life entry
  public struct LifeEntry
  {
    // Player skin
    public RuntimeAnimatorController skin;

    // Where it started
    public Vector2 startingPosition;

    // All the input entries registered in it
    public Queue<InputEntry> inputEntries;

    // When it triggered the aggro
    public float? aggroTime;

    // When it died
    public float deathTime;

    public LifeEntry(Vector2 startingPosition, RuntimeAnimatorController skin)
    {
      this.startingPosition = startingPosition;
      this.skin = skin;

      inputEntries = new Queue<InputEntry>();

      deathTime = 0f;
      aggroTime = null;
    }

    public void RegisterInput(InputEntry entry)
    {
      inputEntries.Enqueue(entry);
    }

    static public LifeEntry Copy(LifeEntry lifeEntry)
    {
      var newEntry = new LifeEntry(lifeEntry.startingPosition, lifeEntry.skin);

      newEntry.inputEntries = new Queue<InputEntry>(lifeEntry.inputEntries);

      newEntry.aggroTime = lifeEntry.aggroTime;
      newEntry.deathTime = lifeEntry.deathTime;

      return newEntry;
    }
  }


  // === STATE

  // List of all life entries recorded so far
  List<LifeEntry> lifeEntries;

  // Life entry being currently recorded
  LifeEntry currentLifeEntry;

  bool disregardCurrentLife = false;


  // === REFS

  static PlayerLifeTracker instance;


  // === INTERFACE

  public void EraseEntries()
  {
    lifeEntries.Clear();
    disregardCurrentLife = true;
  }


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

  private void Awake()
  {
    lifeEntries = new List<LifeEntry>();
  }

  private void Start()
  {
    SingletonCheck();
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoad;
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoad;

    // Clean up

    PlayerController player = FindObjectOfType<PlayerController>();

    // Listen to player spawn
    player?.OnSpawnPlayer.RemoveListener(StartNewLifeEntry);

    // Listen to it's input
    player?.OnPlayerMove.RemoveListener(RegisterMove);
    player?.OnPlayerSprint.RemoveListener(RegisterSprint);

    // Listen for aggro
    FindObjectOfType<SuspicionMeter>()?.OnAggro.RemoveListener(RegisterAggro);

    // Listen for it's death
    player?.GetComponentInChildren<LaserVulnerable>()?.OnDeath.RemoveListener(FinishLifeEntry);
  }

  private void OnSceneLoad(Scene scene, LoadSceneMode mode)
  {
    SetUpForScene(scene);
  }

  private void SetUpForScene(Scene scene)
  {
    if (scene.name == "EndGame")
    {
      gameObject.SetActive(false);
      Destroy(gameObject);
      return;
    }

    // Set up this scene's tracked life npcs
    SetUpTrackedLifeNpcs();

    PlayerController player = FindObjectOfType<PlayerController>();

    // Listen to player spawn
    player.OnSpawnPlayer.AddListener(StartNewLifeEntry);

    // Listen to it's input
    player.OnPlayerMove.AddListener(RegisterMove);
    player.OnPlayerSprint.AddListener(RegisterSprint);

    // Listen for aggro
    FindObjectOfType<SuspicionMeter>().OnAggro.AddListener(RegisterAggro);

    // Listen for it's death
    player.GetComponentInChildren<LaserVulnerable>().OnDeath.AddListener(FinishLifeEntry);
  }

  private void SetUpTrackedLifeNpcs()
  {
    foreach (var lifeEntry in lifeEntries) FindObjectOfType<NpcManager>().UseTrackedLifeNpc(lifeEntry);
  }

  private void StartNewLifeEntry(Vector2 startingPosition)
  {
    disregardCurrentLife = false;

    currentLifeEntry = new LifeEntry(
      startingPosition, FindObjectOfType<PlayerController>().GetComponent<Skin>().ActiveSkin
    );
  }

  private void RegisterMove(Vector2 movementDirection)
  {
    if (disregardCurrentLife) return;

    currentLifeEntry.RegisterInput(new MovementInput(Time.timeSinceLevelLoad, movementDirection));
  }

  private void RegisterSprint(bool toggle)
  {
    if (disregardCurrentLife) return;

    currentLifeEntry.RegisterInput(new SprintInput(Time.timeSinceLevelLoad, toggle));
  }

  private void RegisterAggro()
  {
    if (disregardCurrentLife) return;

    currentLifeEntry.aggroTime = Time.timeSinceLevelLoad;
  }

  private void FinishLifeEntry()
  {
    if (disregardCurrentLife == false)
    {
      currentLifeEntry.deathTime = Time.timeSinceLevelLoad;

      lifeEntries.Add(currentLifeEntry);
    }

    disregardCurrentLife = false;
  }
}

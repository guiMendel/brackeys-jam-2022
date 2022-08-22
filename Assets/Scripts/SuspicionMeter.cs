using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class SuspicionMeter : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How much suspicion each unit of distance from confine are accrues per second")]
  public float confinementBreakSuspicion = 20f;

  [Tooltip("How much suspicion lowers each second")]
  public float suspicionDecay = 5f;

  [Tooltip("How many aliens to spawn on aggro")]
  public int alienCount = 6;

  [Tooltip("Alien object to spawn")]
  public GameObject alien;


  // === STATE

  // Whether already triggered
  public bool Triggered { get; private set; } = false;


  // === PROPERTIES

  // How suspicious the player is. Ranges from 0 to 100
  public float SuspicionLevel { get; private set; } = 0f;

  // Speed at which the suspicion is raising this frame
  public float SuspicionRaise { get; private set; } = 0f;


  // === REFS

  static SuspicionMeter instance;
  AreaConfine playerConfine;


  private void Awake()
  {
    playerConfine = FindObjectOfType<PlayerController>()
      .GetComponent<AreaConfine>();

    EnsureNotNull.Objects(playerConfine);
  }

  private void Update()
  {
    // Raise suspicion
    if (SuspicionRaise != 0f)
    {
      SuspicionLevel += SuspicionRaise * Time.deltaTime;

      // Check for aggro trigger
      if (SuspicionLevel >= 100f) TriggerAggro();
    }

    // Slow suspicion decay
    else SuspicionLevel -= suspicionDecay * Time.deltaTime;

    // Clamp it
    SuspicionLevel = Mathf.Clamp(SuspicionLevel, 0f, 100f);
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
  }

  private void OnEnable()
  {
    SingletonCheck();
    playerConfine.OnOutsideConfinement.AddListener(RaiseSuspicion);
    playerConfine.OnEnterConfinement.AddListener(StopRaising);
  }

  private void OnDisable()
  {
    playerConfine.OnOutsideConfinement.RemoveListener(RaiseSuspicion);
    playerConfine.OnEnterConfinement.RemoveListener(StopRaising);
  }

  private void StopRaising()
  {
    SuspicionRaise = 0f;
  }

  private void RaiseSuspicion(Vector2 areaDistance)
  {
    SuspicionRaise = areaDistance.magnitude * confinementBreakSuspicion;
  }

  private void TriggerAggro()
  {
    if (Triggered) return;

    Triggered = true;

    // No more decay
    suspicionDecay = 0f;

    // Select some regular looking dudes to convert to aliens
    NpcController[] npcs = FindObjectsOfType<NpcController>();

    // Make sure no more aliens than npcs
    if (alienCount > npcs.Length) alienCount = npcs.Length;

    // Sample some aliens
    List<int> alienIndices = new List<int>();

    while (alienIndices.Count < alienCount)
    {
      int alienIndex = Random.Range(0, alienCount);

      if (alienIndices.Contains(alienIndex) == false)
      {
        alienIndices.Add(alienIndex);
        TurnIntoAlien(npcs[alienIndex]);
      }
    }
  }

  private void TurnIntoAlien(NpcController npc)
  {
    // Create alien where the npc is
    Instantiate(alien, npc.transform.position, npc.transform.rotation, npc.transform.parent);

    // Remove the npc
    npc.gameObject.SetActive(false);
    Destroy(npc.gameObject);
  }
}

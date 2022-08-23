using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AlienTargetManager : MonoBehaviour
{
  // === PARAMS

  [Tooltip("How many aliens to spawn on aggro")]
  public int alienCount = 6;

  [Tooltip("Alien object to spawn")]
  public GameObject alien;


  // === STATE

  // Objects which are actually infiltrated aliens
  public List<GameObject> Aliens { get; private set; } = new List<GameObject>();

  // Currently active targets
  public List<GameObject> ActiveTargets { get; private set; } = new List<GameObject>();

  // Whether the aliens are active
  bool aliensActive = false;


  // === REFS

  NpcManager npcManager;


  // === INTERFACE

  public void AddTarget(GameObject target)
  {
    ActiveTargets.Add(target);

    ActivateAliens();

    // When target perishes, remove it from list
    TrackDeath(target);
  }

  private void TrackDeath(GameObject target)
  {
    target.GetComponentInChildren<LaserVulnerable>().OnDeath.AddListener(() => RemoveTarget(target));
  }

  private void Awake()
  {
    npcManager = FindObjectOfType<NpcManager>();

    EnsureNotNull.Objects(npcManager);
  }

  private void RemoveTarget(GameObject target)
  {
    ActiveTargets.Remove(target);

    if (ActiveTargets.Count == 0) HideAliens();
  }

  internal void SwitchTarget(GameObject oldTarget, GameObject newTarget)
  {
    ActiveTargets[ActiveTargets.IndexOf(oldTarget)] = newTarget;

    TrackDeath(newTarget);
  }

  private void HideAliens()
  {
    if (aliensActive == false) return;

    aliensActive = false;

    for (int i = 0; i < Aliens.Count; i++)
    {
      // Ensure it's still alive
      if (Aliens[i] == null)
      {
        Aliens.RemoveAt(i--);
        continue;
      }

      Aliens[i] = TurnIntoNpc(Aliens[i]);
    }
  }

  private GameObject TurnIntoNpc(GameObject alien)
  {
    // Create alien where the npc is
    GameObject newNpc = npcManager.CreateNpcAt(alien.transform.position);

    // Remove the alien
    alien.SetActive(false);
    Destroy(alien);

    return newNpc;
  }

  private void ActivateAliens()
  {
    if (aliensActive) return;

    aliensActive = true;

    for (int i = 0; i < Aliens.Count; i++)
    {
      // Ensure it's still alive
      if (Aliens[i] == null)
      {
        Aliens.RemoveAt(i--);
        continue;
      }

      Aliens[i] = TurnIntoAlien(Aliens[i]);
    }
  }

  private GameObject TurnIntoAlien(GameObject npc)
  {
    // Create alien where the npc is
    GameObject newAlien = Instantiate(alien, npc.transform.position, npc.transform.rotation, npc.transform.parent);

    // Remove the npc
    npc.SetActive(false);
    Destroy(npc);

    return newAlien;
  }

  private void Start()
  {
    // Allocate aliens
    AllocateAliens();
  }

  // Ensures there are aliens ready for the hunt
  private void AllocateAliens()
  {
    // Select some regular looking dudes to convert to aliens
    List<NpcController> npcs = FindObjectsOfType<NpcController>().ToList();

    // If need be, create some more npcs
    while (alienCount > npcs.Count)
    {
      npcs.Add(FindObjectOfType<NpcManager>().CreateNpc().GetComponent<NpcController>());
    }

    // Sample some aliens
    List<int> alienIndices = new List<int>();

    while (alienIndices.Count < alienCount)
    {
      int alienIndex = Random.Range(0, alienCount);

      if (alienIndices.Contains(alienIndex) == false)
      {
        alienIndices.Add(alienIndex);
        Aliens.Add(npcs[alienIndex].gameObject);
      }
    }
  }
}

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

  [Tooltip("Modifier to apply to movement angle offset on aliens spawned")]
  public float angleOffsetModifier = 1f;

  [Tooltip("How long aliens may take to respawn")]
  public Vector2 respawnTime = new Vector2(0.5f, 2f);

  [Tooltip("How long aliens may take to appear")]
  public Vector2 appearTime = new Vector2(0f, 0.6f);

  [Tooltip("How long aliens may take to hide")]
  public Vector2 hideTime = new Vector2(0.2f, 1.5f);


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
    TrackTargetDeath(target);
  }

  public void SwitchAlienObject(GameObject oldObject, GameObject newObject)
  {
    Aliens.Remove(oldObject);
    Aliens.Add(newObject);

    TrackAlienDeath(newObject);
  }

  public void SwitchTarget(GameObject oldTarget, GameObject newTarget)
  {
    ActiveTargets.Remove(oldTarget);
    ActiveTargets.Add(newTarget);

    TrackTargetDeath(newTarget);
  }

  public AlienController CreateAlienAt(Transform npc)
  {
    return Instantiate(
      alien, npc.transform.position, npc.transform.rotation, npc.transform.parent
    ).GetComponent<AlienController>();
  }


  private void TrackAlienDeath(GameObject alien)
  {
    alien.GetComponentInChildren<LaserVulnerable>().OnDeath.AddListener(() => StartCoroutine(ReplenishAlien(alien)));
  }

  private void TrackTargetDeath(GameObject target)
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

      AlienController alien = Aliens[i].GetComponent<AlienController>();

      // If is in alien form, turn into npc
      if (alien != null) alien.TurnIntoNpc(Random.Range(hideTime.x, hideTime.y));
      // Otherwise, cancel a potential turn into alien request
      else Aliens[i].GetComponent<NpcController>().CancelTurn();
    }
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

      NpcController npc = Aliens[i].GetComponent<NpcController>();

      // If is in npc form, turn into alien
      if (npc != null) npc.TurnIntoAlien(Random.Range(appearTime.x, appearTime.y));
      // Otherwise, cancel a potential turn into npc request
      else Aliens[i].GetComponent<AlienController>().CancelTurn();
    }
  }

  private void Start()
  {
    // Allocate aliens
    SpawnAliens();
  }

  // Ensures there are aliens ready for the hunt
  private void SpawnAliens()
  {
    while (alienCount > Aliens.Count) SpawnAlien(skipIntroAnimation: true);
  }

  private void SpawnAlien(bool skipIntroAnimation = false)
  {
    // Create it
    GameObject newNpc = npcManager.CreateNpc(skipIntroAnimation);

    // Add it to our list
    Aliens.Add(newNpc);

    newNpc.tag = "Alien";

    // Listen to it's death
    TrackAlienDeath(newNpc);
  }

  private IEnumerator ReplenishAlien(GameObject deadAlien)
  {
    Aliens.Remove(deadAlien);

    yield return new WaitForSeconds(Random.Range(respawnTime.x, respawnTime.y));

    SpawnAlien();
  }
}

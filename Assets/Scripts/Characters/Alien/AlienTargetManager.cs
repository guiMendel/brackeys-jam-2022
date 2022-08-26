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

  [Tooltip("How often to check for new aliens to join the hunt when there are targets")]
  public float alienAssembleFrequency = 1f;


  // === STATE

  // Objects which are actually infiltrated aliens
  public List<GameObject> Aliens { get; private set; } = new List<GameObject>();

  // Currently active targets
  public List<GameObject> ActiveTargets { get; private set; } = new List<GameObject>();

  Coroutine newAliensAssembleCoroutine = null;


  // === REFS

  NpcManager npcManager;


  // === INTERFACE

  public void AddTarget(GameObject target)
  {
    ActiveTargets.Add(target);

    ActivateAliens();

    // When target perishes, remove it from list
    TrackTargetDeath(target);

    // Start assembling new aliens every so often
    if (newAliensAssembleCoroutine == null)
    {
      newAliensAssembleCoroutine = StartCoroutine(AssembleNewAliens());
    }
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
    if (newAliensAssembleCoroutine != null)
    {
      StopCoroutine(newAliensAssembleCoroutine);
      newAliensAssembleCoroutine = null;
    }

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
    for (int i = 0; i < Aliens.Count; i++)
    {
      // Ensure it's still alive
      if (Aliens[i] == null)
      {
        Aliens.RemoveAt(i--);
        continue;
      }

      AlienController alien = Aliens[i].GetComponent<AlienController>();

      // If is in alien form, cancel a potential turn into npc request
      if (alien != null) alien.CancelTurn();
      // Otherwise, turn into alien
      else Aliens[i].GetComponent<NpcController>().TurnIntoAlien(Random.Range(appearTime.x, appearTime.y));
    }
  }

  private void Start()
  {
    // Allocate aliens
    SpawnAliens();
  }

  IEnumerator AssembleNewAliens()
  {
    while (ActiveTargets.Count > 0)
    {
      ActivateAliens();

      yield return new WaitForSeconds(alienAssembleFrequency);
    }

    newAliensAssembleCoroutine = null;
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

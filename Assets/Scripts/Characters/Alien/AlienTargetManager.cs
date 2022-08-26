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

  public void SwitchTarget(GameObject oldTarget, GameObject newTarget)
  {
    ActiveTargets.Remove(oldTarget);
    ActiveTargets.Add(newTarget);

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
    // Stop if is in move animation
    if (alien.GetComponent<SpawnAnimationScript>() != null) return alien;

    // Create alien where the npc is
    GameObject newNpc = npcManager.CreateNpcAt(alien.transform.position);

    TrackAlienDeath(newNpc);
    newNpc.tag = "Alien";

    // Give it's skin back
    newNpc.GetComponent<Skin>().ActiveSkin = alien.GetComponent<AlienController>().NpcSkin;

    // Remove the alien
    alien.SetActive(false);
    Destroy(alien);

    // Play the npc's particles
    newNpc.GetComponent<ParticleSystem>().Play();

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
    // Stop if is in move animation
    if (npc.GetComponent<SpawnAnimationScript>() != null) return npc;

    // Create alien where the npc is
    AlienController newAlien = Instantiate(
      alien, npc.transform.position, npc.transform.rotation, npc.transform.parent
    ).GetComponent<AlienController>();

    // Store the npc skin
    newAlien.NpcSkin = npc.GetComponent<Skin>().ActiveSkin;

    // Remove the npc
    npc.SetActive(false);
    Destroy(npc);

    // Apply modifier
    newAlien.maxOffset = newAlien.maxOffset * angleOffsetModifier;

    TrackAlienDeath(newAlien.gameObject);
    alien.tag = "Alien";

    return newAlien.gameObject;
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

  private void TrackAlienDeath(GameObject alien)
  {
    alien.GetComponentInChildren<LaserVulnerable>().OnDeath.AddListener(() => StartCoroutine(ReplenishAlien(alien)));
  }

  private IEnumerator ReplenishAlien(GameObject deadAlien)
  {
    Aliens.Remove(deadAlien);

    yield return new WaitForSeconds(Random.Range(respawnTime.x, respawnTime.y));

    SpawnAlien();
  }
}

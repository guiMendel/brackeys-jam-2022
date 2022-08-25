using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
  // === PARAMS

  [Tooltip("How many NPCs to spawn")]
  [Range(0, 100)] public int npcCount = 10;

  [Tooltip("Prefab of a regular NPC")]
  public GameObject npcPrefab;

  [Tooltip("Prefab of an NPC that follows a tracked life record")]
  public GameObject trackedLifeNpcPrefab;

  [Tooltip("Binds the NPC's to this box")]
  public BoxCollider2D movementArea;

  [Tooltip("Where NPCs spawn from at runtime")]
  public List<Transform> runtimeSpawnOrigin;

  [Tooltip("Where NPCs spawned at runtime walk to")]
  public Transform runtimeSpawnDestination;

  [Tooltip("How long npcs may take to respawn")]
  public Vector2 respawnTime = new Vector2(0.5f, 2f);


  // === STATE

  // Npcs using tracked life records
  List<PlayerLifeTracker.LifeEntry> trackedLifeNpcs;


  private void Awake()
  {
    trackedLifeNpcs = new List<PlayerLifeTracker.LifeEntry>();
  }

  // Start is called before the first frame update
  void Start()
  {
    // Spawn regular npcs
    for (int i = 0; i < npcCount; i++) CreateNpc(skipIntroAnimation: true);

    // Spawn tracked life npcs
    foreach (var trackedNpc in trackedLifeNpcs)
    {
      // Spawn it
      GameObject trackedNpcObject = Instantiate(
        trackedLifeNpcPrefab, trackedNpc.startingPosition, Quaternion.identity, transform
      );

      // Give it access to the script
      trackedNpcObject.GetComponent<TrackedLifeRepeater>().SetLifeEntry(trackedNpc);
    }
  }

  // Uses the given tracked life as an npc
  public void UseTrackedLifeNpc(PlayerLifeTracker.LifeEntry lifeEntry)
  {
    trackedLifeNpcs.Add(lifeEntry);
  }

  public GameObject CreateNpc(bool skipIntroAnimation = false)
  {
    Vector2 position;
    skipIntroAnimation = skipIntroAnimation
      || runtimeSpawnOrigin == null
      || runtimeSpawnDestination == null;

    if (skipIntroAnimation)
    {
      // Get a position for it
      position = new Vector2(
        Random.Range(movementArea.bounds.min.x + 1, movementArea.bounds.max.x - 1),
        Random.Range(movementArea.bounds.min.y + 1, movementArea.bounds.max.y - 1)
      );
    }
    else position = runtimeSpawnOrigin[Random.Range(0, runtimeSpawnOrigin.Count)].position;

    // Create it 
    GameObject npc = CreateNpcAt(position);

    if (skipIntroAnimation == false)
    {
      // Add the intro animation script
      SpawnAnimationScript animation = npc.AddComponent<SpawnAnimationScript>();
      animation.destination = runtimeSpawnDestination;
    }

    // Track npc count
    TrackDeath(npc);

    return npc;
  }

  public GameObject CreateNpcAt(Vector2 position)
  {
    GameObject newNpc = Instantiate(npcPrefab, position, Quaternion.identity, transform);

    newNpc.GetComponent<AreaConfine>().movementArea = movementArea;

    return newNpc;
  }

  private void TrackDeath(GameObject npc)
  {
    npc.GetComponentInChildren<LaserVulnerable>().OnDeath.AddListener(() => StartCoroutine(RespawnNpcs()));
  }

  private IEnumerator RespawnNpcs()
  {
    while (true)
    {
      yield return new WaitForSeconds(Random.Range(respawnTime.x, respawnTime.y));

      int npcsAlive = FindObjectsOfType<NpcController>()
        .Where(npc => npc.CompareTag("Alien") == false)
        .ToArray().Length - 1;

      if (npcsAlive < npcCount) CreateNpc();
      else break;
    }
  }
}

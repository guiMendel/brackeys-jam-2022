using System.Collections;
using System.Collections.Generic;
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
    for (int i = 0; i < npcCount; i++) CreateNpc();

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

  public GameObject CreateNpc()
  {
    // Get a position for it
    Vector2 position = new Vector2(
      Random.Range(movementArea.bounds.min.x + 1, movementArea.bounds.max.x - 1),
      Random.Range(movementArea.bounds.min.y + 1, movementArea.bounds.max.y - 1)
    );

    return CreateNpcAt(position);
  }

  public GameObject CreateNpcAt(Vector2 position)
  {
    GameObject newNpc = Instantiate(npcPrefab, position, Quaternion.identity, transform);

    newNpc.GetComponent<AreaConfine>().movementArea = movementArea;

    return newNpc;
  }
}

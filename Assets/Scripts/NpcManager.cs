using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcManager : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("How many NPCs to spawn")]
  [Range(0, 100)] public int npcCount = 10;

  [Tooltip("Prefab of an NPC")]
  public GameObject npc;

  [Tooltip("Binds the NPC's to this box")]
  public BoxCollider2D movementArea;


  // Start is called before the first frame update
  void Start()
  {
    // Spawn the npcs
    for (int i = 0; i < npcCount; i++)
    {
      // Get a position for it
      Vector2 position = new Vector2(
        Random.Range(movementArea.bounds.min.x + 1, movementArea.bounds.max.x - 1),
        Random.Range(movementArea.bounds.min.y + 1, movementArea.bounds.max.y - 1)
      );

      GameObject newNpc = Instantiate(npc, position, Quaternion.identity, transform);

      newNpc.GetComponent<NpcController>().movementArea = movementArea;
    }
  }
}

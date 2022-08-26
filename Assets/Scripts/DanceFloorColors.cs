using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DanceFloorColors : MonoBehaviour
{
  // === PARAMS

  public BoxCollider2D danceArea;

  public float changeFrequency = 0.5f;

  public float saturation = 0.2f;

  public float colorValue = 1f;


  // === REFS

  Tilemap floorMap;


  private void Awake()
  {
    floorMap = GetComponent<Tilemap>();

    EnsureNotNull.Objects(floorMap);
  }


  private IEnumerator Start()
  {
    while (true)
    {
      // Loop through the row's center y coordinates
      for (
        float row = danceArea.bounds.min.y + 0.5f;
        row < danceArea.bounds.max.y;
        row++
      )
      {
        // Loop through the columns's center x coordinates
        for (
          float column = danceArea.bounds.min.x + 0.5f;
          column < danceArea.bounds.max.x;
          column++
        )
        {
          // Set the corresponding tile's color
          ChangeColor(row, column);
        }
      }

      yield return new WaitForSeconds(1f / changeFrequency);
    }
  }

  private void ChangeColor(float row, float column)
  {
    // Get the cell
    Vector3Int cell = floorMap.layoutGrid.WorldToCell(new Vector3(column, row, 0f));

    // Remove lock color flag
    floorMap.SetTileFlags(cell, TileFlags.None);

    // Set it's color
    floorMap.SetColor(cell, Color.HSVToRGB(Random.value, saturation, colorValue));
  }
}

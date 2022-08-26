using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoxCollider2DExtension
{
  public static Vector2 Accommodate(this BoxCollider2D collider2D, Collider2D otherCollider)
  {
    return new Vector2(
      Random.Range(
        collider2D.bounds.min.x + otherCollider.bounds.extents.x, collider2D.bounds.max.x - otherCollider.bounds.extents.x
      ) + otherCollider.transform.position.x - otherCollider.bounds.center.x,
      Random.Range(
        collider2D.bounds.min.y + otherCollider.bounds.extents.y, collider2D.bounds.max.y - otherCollider.bounds.extents.y
      ) + otherCollider.transform.position.y - otherCollider.bounds.center.y
    );
  }
  public static Vector2 Accommodate(this Bounds bounds, Collider2D otherCollider)
  {
    return new Vector2(
      Random.Range(
        bounds.min.x + otherCollider.bounds.extents.x, bounds.max.x - otherCollider.bounds.extents.x
      ) + otherCollider.transform.position.x - otherCollider.bounds.center.x,
      Random.Range(
        bounds.min.y + otherCollider.bounds.extents.y, bounds.max.y - otherCollider.bounds.extents.y
      ) + otherCollider.transform.position.y - otherCollider.bounds.center.y
    );
  }
}

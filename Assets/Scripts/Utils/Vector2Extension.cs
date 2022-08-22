using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2Extension
{
  public static Vector2 Rotated(this Vector2 vector, float degrees)
  {
    return Quaternion.Euler(0, 0, degrees) * vector;
  }

  public static Vector2 RandomRotated(this Vector2 vector)
  {
    return vector.Rotated(UnityEngine.Random.Range(0f, 360f));
  }
}

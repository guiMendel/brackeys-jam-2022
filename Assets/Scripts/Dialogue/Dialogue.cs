using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenuAttribute(fileName = "DialogueData", menuName = "ScriptableObjects/Dialogue")]
public class Dialogue : ScriptableObject
{
  [Tooltip("The dialogue text")]
  [TextArea(5, 20)] public string text;

  [Tooltip("Delay to wait before displaying dialogue")]
  [Min(0f)] public float delay = 0f;
}

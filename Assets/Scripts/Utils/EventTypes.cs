using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Event
{
  [Serializable] public class Vector2 : UnityEvent<UnityEngine.Vector2> { }
  [Serializable] public class Bool : UnityEvent<bool> { }
}
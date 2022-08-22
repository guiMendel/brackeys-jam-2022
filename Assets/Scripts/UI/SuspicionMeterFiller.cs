using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SuspicionMeterFiller : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("Raise speed to activate yellow warning, in %")]
  public float yellowWarningThreshold = 20f;

  [Tooltip("Raise speed to activate red warning, in %")]
  public float redWarningThreshold = 40f;


  // === REFS

  VisualElement meterBar;
  VisualElement meterOverlay;
  SuspicionMeter suspicionMeter;

  private void Awake()
  {
    meterBar = FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("suspicion-bar");
    meterOverlay = meterBar.Q<VisualElement>("overlay");
    suspicionMeter = FindObjectOfType<SuspicionMeter>();

    EnsureNotNull.Objects(meterOverlay, suspicionMeter);
  }

  private void Update()
  {
    meterOverlay.style.height = new Length(suspicionMeter.SuspicionLevel, LengthUnit.Percent);

    meterBar.ClearClassList();

    // print((suspicionMeter.SuspicionRaise, yellowWarningThreshold));

    // Raise speed visual queue
    if (suspicionMeter.SuspicionRaise >= redWarningThreshold)
    {
      meterBar.AddToClassList("red-warning");
    }

    else if (suspicionMeter.SuspicionRaise >= yellowWarningThreshold)
    {
      meterBar.AddToClassList("yellow-warning");
    }
  }
}
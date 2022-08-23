using System;
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


  // === STATE

  bool suspicionMeterWasEnabled = true;


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
    if (suspicionMeter.enabled == false)
    {
      if (suspicionMeterWasEnabled) HideMeter();
      return;
    }
    else if (suspicionMeterWasEnabled == false) ShowMeter();

    meterOverlay.style.height = new Length(suspicionMeter.SuspicionLevel, LengthUnit.Percent);

    meterBar.ClearClassList();

    // Raise speed visual queue
    if (suspicionMeter.SuspicionRaise >= redWarningThreshold || suspicionMeter.Triggered)
    {
      meterBar.AddToClassList("red-warning");
    }

    else if (suspicionMeter.SuspicionRaise >= yellowWarningThreshold)
    {
      meterBar.AddToClassList("yellow-warning");
    }
  }

  private void ShowMeter()
  {
    suspicionMeterWasEnabled = true;
    meterBar.style.display = StyleKeyword.Initial;
  }

  private void HideMeter()
  {
    suspicionMeterWasEnabled = false;
    meterBar.style.display = StyleKeyword.None;
  }
}

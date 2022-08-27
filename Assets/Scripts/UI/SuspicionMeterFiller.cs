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

  [Tooltip("Whether to hide meter")]
  public bool hideMeter = false;


  // === STATE

  bool meterIsDisplayed = true;


  // === REFS

  VisualElement meterBar;
  VisualElement meterOverlay;
  SuspicionMeter suspicionMeter;

  private void Awake()
  {
    meterBar = FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("suspicion-bar");
    meterOverlay = meterBar.Q<VisualElement>("overlay");
    suspicionMeter = FindObjectOfType<SuspicionMeter>();

    EnsureNotNull.Objects(meterOverlay);
  }

  private void Update()
  {
    if (suspicionMeter == null || suspicionMeter.enabled == false || hideMeter)
    {
      if (meterIsDisplayed) HideMeter();
      return;
    }
    else if (meterIsDisplayed == false) ShowMeter();

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
    meterIsDisplayed = true;
    meterBar.style.display = StyleKeyword.Initial;
  }

  private void HideMeter()
  {
    meterIsDisplayed = false;
    meterBar.style.display = StyleKeyword.None;
  }
}

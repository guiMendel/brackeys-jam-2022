using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClubDialogues : MonoBehaviour
{
  // === REFS

  PlayerController playerController => FindObjectOfType<PlayerController>();
  LaserVulnerable playerVulnerable => playerController?.GetComponentInChildren<LaserVulnerable>();
  DialogueHandler handler => GetComponent<DialogueHandler>();
  SuspicionMeter suspicionMeter => FindObjectOfType<SuspicionMeter>();
  SuspicionMeterFiller meterFiller => FindObjectOfType<SuspicionMeterFiller>();
}

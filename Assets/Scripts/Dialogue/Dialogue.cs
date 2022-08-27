using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenuAttribute(fileName = "DialogueData", menuName = "ScriptableObjects/Dialogue")]
public class Dialogue : ScriptableObject
{
  [Header("Content")]

  [Tooltip("The dialogue text")]
  [TextArea(5, 5000)] public string text;

  [Tooltip("Delay to wait before displaying dialogue")]
  [Min(0f)] public float delay = 0f;

  [Tooltip("Max time the dialogue may appear (0 means unlimited time)")]
  [Min(0f)] public float maxDuration = 0f;

  [Tooltip("Sets up these strings as the current tooltips (empty leaves it unchanged)")]
  public string[] setTooltips;

  [Tooltip("Resets tooltips")]
  public bool resetTooltips = false;


  [Header("Style")]

  [Tooltip("Modifier to apply to writing speed")]
  [Min(0.1f)] public float speedModifier = 1f;

  [Tooltip("Modifier to apply to letter sound pitch")]
  [Min(0.1f)] public float pitchModifier = 1f;

  [Tooltip("Modifier to apply to font size")]
  [Min(0.1f)] public float fontSizeModifier = 1f;


  [Header("Follow Up")]

  [Tooltip("Whether this dialogue passes automatically")]
  public bool autoSkip = true;

  [Tooltip("Dialogue to follow up on this one")]
  [SerializeField] Dialogue followUp;

  [Tooltip("Delay to wait before following up")]
  [Min(0f)] public float followUpDelay = 1f;


  [Header("Hooks")]

  [Tooltip("Fires when dialogue starts")]
  public UnityEvent OnStart;

  [Tooltip("Fires when dialogue stops, for whatever reason")]
  public UnityEvent OnStop;

  [Tooltip("Fires when dialogue finishes normally")]
  public UnityEvent OnFinish;

  [Tooltip("Fires when dialogue is interrupted")]
  public UnityEvent OnInterrupt;

  [Tooltip("Fires when the follow up is fetched")]
  public UnityEvent OnFollowUp;

  [Tooltip("Fires when the dialogue leaves the screen (either on interrupt, on follow up, or on maxDuration timeout")]
  public UnityEvent OnLeave;


  // === STATE
  bool _started;
  bool _finished;
  bool _interrupted;


  // Whether it has already bene started
  public bool Started
  {
    get { return _started; }
    set
    {
      _started = value;
      if (value) OneShotInvoke(OnStart);
    }
  }

  // Whether it has finished normally
  public bool Finished
  {
    get { return _finished; }
    set
    {
      _finished = value;
      if (value)
      {
        OneShotInvoke(OnFinish);
        OneShotInvoke(OnStop);
        // Dont invoke on leave because it hasn't necessarily left yet
      }
    }
  }

  // Whether it has been interrupted
  public bool Interrupted
  {
    get { return _interrupted; }
    set
    {
      _interrupted = value;
      if (value)
      {
        OneShotInvoke(OnInterrupt);
        OneShotInvoke(OnStop);
      }
    }
  }

  // Whether it has already stopped, for whatever reason
  public bool Stopped => Finished || Interrupted;

  public bool HasFollowUp => followUp != null;

  public Dialogue GetFollowUp()
  {
    OneShotInvoke(OnFollowUp);
    return followUp;
  }

  public void RegisterLeave() { OneShotInvoke(OnLeave); }


  private void OneShotInvoke(UnityEvent unityEvent)
  {
    unityEvent.Invoke();
    unityEvent.RemoveAllListeners();
  }

  public void Reset()
  {
    _started = _finished = _interrupted = false;
  }
}

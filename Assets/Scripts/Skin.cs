using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Animation controller for each available skin")]
  public List<RuntimeAnimatorController> skins;


  // === PROPERTIES

  // Currently active skin
  public RuntimeAnimatorController ActiveSkin
  {
    get { return animator.runtimeAnimatorController; }
    set
    {
      skinInitialized = true;
      animator.runtimeAnimatorController = value;
    }
  }


  // === STATE

  // Whether the skin has been initialized already
  bool skinInitialized = false;


  // === REFS

  Animator animator;


  private void Awake()
  {
    animator = GetComponent<Animator>();
    if (skinInitialized == false) PickSkin();

    EnsureNotNull.Objects(animator);
  }

  private void PickSkin()
  {
    ActiveSkin = skins[Random.Range(0, skins.Count)];
  }
}

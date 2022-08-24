using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UICurtain : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Automatically open curtain on start")]
  public bool openOnLoad = true;

  public float accelerationDerivative = 10f;

  public float initialAcceleration = 2f;


  // === STATE

  float speed = 0f;
  float acceleration = 0f;


  // === PROPERTIES

  float Position
  {
    get { return curtain.style.left.value.value; }
    set { curtain.style.left = new Length(Mathf.Clamp(value, -100, 100), LengthUnit.Percent); }
  }


  // === REFS

  VisualElement curtain;


  private void Awake()
  {
    curtain = FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("curtain");
  }

  private void Update()
  {
    if (speed == 0) return;

    Position = Position + speed * Time.deltaTime;
  }

  private void Start()
  {
    // Have it initially cover the screen
    Position = 0f;

    // Remove curtain
    if (openOnLoad) Open();
  }

  public void Close() { StartCoroutine(CloseCoroutine()); }
  public void Open() { StartCoroutine(OpenCoroutine()); }

  private IEnumerator OpenCoroutine()
  {
    Position = 0f;

    acceleration = initialAcceleration;
    speed = 0f;

    while (Position > -100)
    {
      acceleration += accelerationDerivative * Time.deltaTime;
      speed -= acceleration * Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }

    speed = 0f;
    Position = -100f;
  }

  private IEnumerator CloseCoroutine()
  {
    Position = 100f;

    acceleration = initialAcceleration;
    speed = 0f;

    while (Position > 0)
    {
      acceleration += accelerationDerivative * Time.deltaTime;
      speed -= acceleration * Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }

    speed = 0f;
    Position = 0f;
  }

}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UICurtain : MonoBehaviour
{
  // === PARAMS

  public float accelerationDerivative = 10f;

  public float initialAcceleration = 2f;


  // === STATE

  float speed = 0f;
  float acceleration = 0f;


  // === REFS

  VisualElement curtain;


  private void Awake()
  {
    curtain = FindObjectOfType<UIDocument>().rootVisualElement.Q<VisualElement>("curtain");
  }

  private void Update()
  {
    if (speed == 0) return;

    float newLeft = Mathf.Clamp(
      curtain.style.left.value.value + speed * Time.deltaTime, -100, 100
    );

    curtain.style.left = new Length(newLeft, LengthUnit.Percent);
  }

  private void Start()
  {
    // Remove curtain
    Open();
  }

  public void Close() { StartCoroutine(CloseCoroutine()); }
  public void Open() { StartCoroutine(OpenCoroutine()); }

  private IEnumerator OpenCoroutine()
  {
    curtain.style.left = 0f;

    acceleration = initialAcceleration;
    speed = 0f;

    while (curtain.style.left.value.value > -100)
    {
      acceleration += accelerationDerivative * Time.deltaTime;
      speed -= acceleration * Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }

    speed = 0f;
    curtain.style.left = new Length(-100, LengthUnit.Percent);
  }

  private IEnumerator CloseCoroutine()
  {
    curtain.style.left = new Length(100, LengthUnit.Percent);

    acceleration = initialAcceleration;
    speed = 0f;

    while (curtain.style.left.value.value > 0)
    {
      acceleration += accelerationDerivative * Time.deltaTime;
      speed -= acceleration * Time.deltaTime;

      yield return new WaitForEndOfFrame();
    }

    speed = 0f;
    curtain.style.left = new Length(0f);
  }

}

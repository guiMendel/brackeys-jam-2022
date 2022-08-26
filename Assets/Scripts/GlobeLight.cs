using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GlobeLight : MonoBehaviour
{
  // === PARAMS

  public float rotationSpeed = 10f;

  public float colorChangeFrequency = 0.5f;

  public float lightDecaySpeed = 0.1f;


  // === STATE

  float initialLightIntensity;


  // === REFS

  Light2D light2D;


  private void Awake()
  {
    light2D = GetComponent<Light2D>();

    EnsureNotNull.Objects(light2D);
  }


  private void Start()
  {
    initialLightIntensity = light2D.intensity;

    StartCoroutine(ChangeColor());
  }

  private IEnumerator ChangeColor()
  {
    while (true)
    {
      // Get a random hue
      light2D.color = Color.HSVToRGB(Random.value, 1f, 1f);

      // Restore light intensity
      light2D.intensity = initialLightIntensity;

      yield return new WaitForSeconds(1f / colorChangeFrequency);
    }
  }

  private void Update()
  {
    transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

    light2D.intensity = light2D.intensity * (1f - lightDecaySpeed);
  }
}

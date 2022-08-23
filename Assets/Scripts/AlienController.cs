using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AlienController : MonoBehaviour
{
  // === INTERFACE

  [Tooltip("Max shoot range")]
  [Min(0f)] public float shootRange = 4f;

  [Tooltip("Max degrees to vary movement offset, in degrees per second")]
  public float offsetVariation = 1f;

  [Tooltip("Max movement offset, in degrees")]
  public float maxOffset = 15f;

  [Tooltip("Laser to shoot")]
  public GameObject laser;

  [Tooltip("Min and max time alien waits before it starts shooting")]
  public Vector2 warmupTime = new Vector2(0.4f, 0.8f);

  [Tooltip("Min and max reload time, in seconds")]
  public Vector2 reloadTime = new Vector2(1.5f, 2.5f);


  // === STATE

  // Movement direction offset
  float movementOffset;


  // === PROPERTIES

  public GameObject Target { get; set; }

  // Whether is reloading
  public bool Reloading { get; private set; }

  // Whether is in range to shoot the target
  public bool InRange =>
    Target == null ? false : Vector2.Distance(transform.position, Target.transform.position) <= shootRange;


  // === REFS

  Movement movement;
  NpcController npcController;


  private void Awake()
  {
    movement = GetComponent<Movement>();
    npcController = GetComponent<NpcController>();

    EnsureNotNull.Objects(movement, npcController);
  }

  private void Start()
  {
    // Disable wandering
    npcController.enabled = false;

    // Warm up time
    StartCoroutine(Reload(Random.Range(warmupTime.x, warmupTime.y)));
  }

  private void Update()
  {
    ChangeOffset();

    if (Target == null)
    {
      npcController.enabled = true;
      return;
    }

    if (InRange) Shoot();
    else Chase();
  }

  private void ChangeOffset()
  {
    movementOffset = Mathf.Clamp(
      movementOffset + Random.Range(-offsetVariation, offsetVariation) * Time.deltaTime,
      -maxOffset,
      maxOffset
    );
  }

  private void Chase()
  {
    // Stop wandering
    npcController.enabled = false;

    Vector2 targetDirection = (Target.transform.position - transform.position).normalized;

    // Set movement towards target
    movement.SetTargetMovement(targetDirection.Rotated(movementOffset));
  }

  private void Shoot()
  {
    // If reloading, move around
    if (Reloading)
    {
      MoveAround();
      return;
    }

    // Stop wandering
    npcController.enabled = false;

    // Stop moving
    movement.SetTargetMovement(Vector2.zero);

    // Shoot
    LaserController newLaser = Instantiate(
      laser, transform.position, Quaternion.identity, GameObject.Find("Projectiles")?.transform
    ).GetComponent<LaserController>();

    newLaser.SetTarget(Target);
    newLaser.Owner = gameObject;

    // Reload
    StartCoroutine(Reload(Random.Range(reloadTime.x, reloadTime.y)));
  }

  private IEnumerator Reload(float time)
  {
    Reloading = true;

    yield return new WaitForSeconds(time);

    Reloading = false;
  }

  private void MoveAround()
  {
    // Enable wandering
    npcController.enabled = true;
  }
}

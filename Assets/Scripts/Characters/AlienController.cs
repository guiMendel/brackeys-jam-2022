using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

  // Whether is reloading
  public bool Reloading { get; private set; }


  // === REFS

  Movement movement;
  NpcController npcController;
  AlienTargetManager alienTargetManager;


  private void Awake()
  {
    movement = GetComponent<Movement>();
    npcController = GetComponent<NpcController>();
    alienTargetManager = FindObjectOfType<AlienTargetManager>();

    EnsureNotNull.Objects(movement, npcController, alienTargetManager);
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
    GameObject target = GetTarget();

    if (target == null)
    {
      npcController.enabled = true;
      return;
    }

    if (InRange(target)) Shoot(target);
    else Chase(target);
  }

  private GameObject GetTarget()
  {
    if (alienTargetManager.ActiveTargets.Count == 0) return null;

    // Get closest target
    return alienTargetManager
      .ActiveTargets
      .OrderBy(target => Vector2.Distance(target.transform.position, transform.position))
      .ElementAt(0);
  }

  // Whether is in range to shoot the target
  public bool InRange(GameObject target)
  {
    return target == null ? false : Vector2.Distance(transform.position, target.transform.position) <= shootRange;
  }

  private void ChangeOffset()
  {
    movementOffset = Mathf.Clamp(
      movementOffset + Random.Range(-offsetVariation, offsetVariation) * Time.deltaTime,
      -maxOffset,
      maxOffset
    );
  }

  private void Chase(GameObject target)
  {
    // Change it's movement offset
    ChangeOffset();

    // Stop wandering
    npcController.enabled = false;

    Vector2 targetDirection = (target.transform.position - transform.position).normalized;

    // Set movement towards target
    movement.SetTargetMovement(targetDirection.Rotated(movementOffset));
  }

  private void Shoot(GameObject target)
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

    newLaser.SetTarget(target);
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

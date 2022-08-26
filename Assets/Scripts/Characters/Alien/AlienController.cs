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

  [Tooltip("Where the laser will come from")]
  public Transform laserOrigin;

  [Tooltip("Element to rotate to point to the alien's target")]
  public Transform shoulder;

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

  // What was the npc skin before transforming into an alien
  public RuntimeAnimatorController NpcSkin { get; set; }

  // Coroutine to turn into npc
  Coroutine turnCoroutine;


  // === PROPERTIES

  // Whether is reloading
  public bool Reloading { get; private set; }


  // === REFS

  Movement movement;
  NpcController npcController;
  AlienTargetManager alienTargetManager;
  Animator gunAnimator;
  ParticleSystem smokeParticles;
  NpcManager npcManager;


  // === INTERFACE

  // Whether is in range to shoot the target
  public bool InRange(GameObject target)
  {
    return target == null ? false : Vector2.Distance(transform.position, target.transform.position) <= shootRange;
  }

  public void CancelTurn()
  {
    if (turnCoroutine == null) return;
    StopCoroutine(turnCoroutine);
    turnCoroutine = null;
  }

  public void TurnIntoNpc(float delay = 0)
  {
    CancelTurn();

    turnCoroutine = StartCoroutine(TurnIntoNpcCoroutine(delay));
  }

  private IEnumerator TurnIntoNpcCoroutine(float delay = 0)
  {
    yield return new WaitForSeconds(delay);

    // Stop if is in move animation
    if (GetComponent<SpawnAnimationScript>() != null) yield break;

    // Create npc where alien is
    GameObject newNpc = npcManager.CreateNpcAt(transform.position);

    // Mark npc as alien
    newNpc.tag = "Alien";

    // Give it's skin back
    if (NpcSkin != null) newNpc.GetComponent<Skin>().ActiveSkin = NpcSkin;

    // Play the npc's particles
    newNpc.GetComponent<ParticleSystem>().Play();

    // Update alien target manager
    alienTargetManager.SwitchAlienObject(gameObject, newNpc);

    // Remove the alien
    gameObject.SetActive(false);
    Destroy(gameObject);
  }


  private void Awake()
  {
    movement = GetComponent<Movement>();
    npcController = GetComponent<NpcController>();
    alienTargetManager = FindObjectOfType<AlienTargetManager>();
    npcManager = FindObjectOfType<NpcManager>();
    gunAnimator = shoulder.GetComponentInChildren<Animator>();
    smokeParticles = shoulder.GetComponentInChildren<ParticleSystem>();

    EnsureNotNull.Objects(movement, npcController, alienTargetManager, gunAnimator, smokeParticles, npcManager);
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
      // Return shoulder to resting position
      shoulder.rotation = Quaternion.identity;

      // Activate npc behavior
      npcController.enabled = true;
      return;
    }

    AimAt(target.transform.position);

    if (InRange(target)) Shoot(target);
    else Chase(target);
  }

  private void AimAt(Vector3 target)
  {
    float targetAngle = Mathf.Atan2(
      target.y - transform.position.y, target.x - transform.position.x
    ) * Mathf.Rad2Deg;

    // If flipped, flip the angle too
    if (transform.localScale.x == -1f)
    {
      // targetAngle *= -1f;
      targetAngle += 180f;
    }

    shoulder.rotation = Quaternion.Euler(0, 0, targetAngle);
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
      laser, laserOrigin.position, Quaternion.identity, GameObject.Find("Projectiles")?.transform
    ).GetComponent<LaserController>();

    newLaser.SetTarget(target);
    newLaser.Owner = gameObject;

    // Reload
    StartCoroutine(Reload(Random.Range(reloadTime.x, reloadTime.y)));

    // Play animation
    gunAnimator.Play("kick");

    // Play particles
    if (transform.localScale.x == -1f)
    {
      smokeParticles.transform.localScale = new Vector3(-1f, 1f, 1f);
    }
    else smokeParticles.transform.localScale = Vector3.one;
    smokeParticles.Emit(20);
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

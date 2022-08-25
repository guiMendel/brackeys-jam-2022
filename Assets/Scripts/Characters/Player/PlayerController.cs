using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Area inside of which the player will be randomly spawned")]
  public BoxCollider2D initialArea;

  [Tooltip("How many seconds to wait before reloading on death")]
  public float deathRestartDelay = 3f;

  [Tooltip("Disables player movement")]
  public bool disableMovement = false;

  [Tooltip("Disables sprinting")]
  public bool disableSprinting = false;

  [Tooltip("Disables all input")]
  public bool disableInput = false;

  public Event.Vector2 OnSpawnPlayer;
  public Event.Vector2 OnPlayerMove;
  public Event.Bool OnPlayerSprint;


  // === REFS

  LaserVulnerable laserVulnerable;
  PlayerCharacter playerCharacter;


  // === INTERFACE

  public void Move(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.started || disableInput || disableMovement) return;

    Vector2 movementVector = callbackContext.ReadValue<Vector2>();

    playerCharacter.Move(movementVector);

    OnPlayerMove.Invoke(movementVector);
  }

  public void Sprint(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.started || disableInput || disableSprinting) return;

    playerCharacter.Sprint(callbackContext.performed);

    OnPlayerSprint.Invoke(callbackContext.performed);
  }

  public void SkipDialogue(InputAction.CallbackContext callbackContext)
  {
    if (callbackContext.performed) FindObjectOfType<DialogueHandler>().Skip();
  }


  private void Awake()
  {
    OnSpawnPlayer ??= new Event.Vector2();
    OnPlayerMove ??= new Event.Vector2();
    OnPlayerSprint ??= new Event.Bool();

    playerCharacter = GetComponent<PlayerCharacter>();
    laserVulnerable = GetComponentInChildren<LaserVulnerable>();

    EnsureNotNull.Objects(laserVulnerable, playerCharacter);
  }

  private void OnEnable()
  {
    laserVulnerable.OnDeath.AddListener(HandleDeath);
  }

  private void OnDisable()
  {
    laserVulnerable.OnDeath.RemoveListener(HandleDeath);
  }

  private void HandleDeath()
  {
    FindObjectOfType<SceneHandler>().ReloadScene(deathRestartDelay);
  }

  private void Start()
  {
    if (initialArea != null) PickSpawnPosition();

    OnSpawnPlayer.Invoke(transform.position);
  }

  private void PickSpawnPosition()
  {
    BoxCollider2D ownCollider = GetComponent<BoxCollider2D>();

    // Place player inside initial area randomly
    transform.position = new Vector2(
      Random.Range(
        initialArea.bounds.min.x + ownCollider.bounds.extents.x, initialArea.bounds.max.x - ownCollider.bounds.extents.x
      ),
      Random.Range(
        initialArea.bounds.min.y + ownCollider.bounds.extents.y, initialArea.bounds.max.y - ownCollider.bounds.extents.y
      ) + transform.position.y - ownCollider.bounds.center.y
    );
  }
}

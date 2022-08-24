using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ScreenTransition : MonoBehaviour
{
  // === PARAMS

  [Tooltip("Where the player will transition to")]
  public Transform characterDestination;

  [Tooltip("Where the camera will transition to")]
  public Transform cameraDestination;

  [Tooltip("Snap distance")]
  public float snapDistance = 0.05f;

  [Tooltip("Whether to set the destinations as the new spawn points")]
  public bool persistAsSpawnPoints;

  [Tooltip("Whether to erase tracked player lives on transition")]
  public bool eraseTrackedLives;

  public UnityEvent OnTransitionStart;
  public UnityEvent OnTransitionEnd;


  // === STATE

  Coroutine transitionCoroutine;


  private void OnCollisionEnter2D(Collision2D other)
  {
    if (transitionCoroutine != null) StopCoroutine(transitionCoroutine);

    transitionCoroutine = StartCoroutine(Transition(other.gameObject.GetComponent<Movement>()));
  }

  private IEnumerator Transition(Movement character)
  {
    OnTransitionStart.Invoke();

    Rigidbody2D characterBody = character.GetComponent<Rigidbody2D>();
    PlayerController playerController = character.GetComponent<PlayerController>();
    Movement camera = Camera.main.GetComponent<Movement>();

    // Whether player has arrived
    bool characterArrived = false;

    // Whether camera has arrived (already starts as true if character is not the current player)
    bool cameraArrived = playerController == null;

    // Disable the character's collisions
    character.SnapTo(character.transform.position);
    characterBody.bodyType = RigidbodyType2D.Static;
    character.useRigidbody2D = false;

    // Disable input
    if (playerController != null) playerController.disableInput = true;

    // Send him on his way
    Move(character, characterDestination.position);
    if (cameraArrived == false) Move(camera, cameraDestination.position);

    while (characterArrived == false || cameraArrived == false)
    {
      if (characterArrived == false) characterArrived = CheckDistance(character, characterDestination.position);
      if (cameraArrived == false) cameraArrived = CheckDistance(camera, cameraDestination.position);

      yield return new WaitForEndOfFrame();
    }

    // Reenable the character's collisions
    characterBody.bodyType = RigidbodyType2D.Dynamic;
    character.useRigidbody2D = true;

    // Reenable input
    if (playerController != null) playerController.disableInput = false;

    OnTransitionEnd.Invoke();

    if (persistAsSpawnPoints) PersistSpawnPoints(characterDestination.position, cameraDestination.position);
    if (eraseTrackedLives) EraseTrackedLives();

    transitionCoroutine = null;
  }

  private void EraseTrackedLives()
  {
    FindObjectOfType<PlayerLifeTracker>().EraseEntries();
  }

  private void PersistSpawnPoints(Vector3 player, Vector3 camera)
  {
    FindObjectOfType<SceneHandler>().SetSpawnPoints(player, camera);
  }

  private bool CheckDistance(Movement movable, Vector2 destination)
  {
    Move(movable, destination);

    float sqrDistance = (destination - (Vector2)movable.transform.position).sqrMagnitude;

    bool arrived = sqrDistance <= snapDistance * snapDistance;

    // Snap
    if (arrived) movable.SnapTo(destination);

    return arrived;
  }

  private void Move(Movement movable, Vector2 destination)
  {
    // Get target direction
    Vector2 targetDirection = (destination - (Vector2)movable.transform.position).normalized;

    // Set his speed
    movable.SetTargetMovement(targetDirection);
  }

  private void OnDrawGizmos()
  {
    Gizmos.color = new Color(0, 1, 0, 0.2f);
    Gizmos.DrawCube(
      GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.extents * 2
    );
  }
}

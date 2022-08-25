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
  public BoxCollider2D characterDestination;

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

  bool setSpawnPointsOnLoad = false;

  // Objects being transitioned
  List<GameObject> objectsInTransition = new List<GameObject>();


  private void OnCollisionEnter2D(Collision2D other)
  {
    // If it's already in transition, ignore
    if (objectsInTransition.Contains(other.gameObject)) return;

    StartCoroutine(Transition(other.gameObject.GetComponent<Movement>()));
  }

  private IEnumerator Transition(Movement character)
  {
    objectsInTransition.Add(character.gameObject);

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

    // Get destination point
    Vector2 destination = characterDestination.Accommodate(character.GetComponent<BoxCollider2D>());

    // Send him on his way
    Move(character, destination);
    if (cameraArrived == false) Move(camera, cameraDestination.position);

    while (characterArrived == false || cameraArrived == false)
    {
      if (characterArrived == false) characterArrived = CheckDistance(character, destination);
      if (cameraArrived == false) cameraArrived = CheckDistance(camera, cameraDestination.position);

      yield return new WaitForEndOfFrame();
    }

    // Reenable the character's collisions
    if (characterBody != null) characterBody.bodyType = RigidbodyType2D.Dynamic;
    if (character != null) character.useRigidbody2D = true;

    // Reenable input
    if (playerController != null) playerController.disableInput = false;

    OnTransitionEnd.Invoke();

    if (persistAsSpawnPoints && playerController != null) PersistSpawnPoints();
    if (eraseTrackedLives && playerController != null) EraseTrackedLives();

    objectsInTransition.Remove(character.gameObject);
  }

  private void EraseTrackedLives()
  {
    FindObjectOfType<PlayerLifeTracker>().EraseEntries();
  }

  private void PersistSpawnPoints()
  {
    setSpawnPointsOnLoad = true;

    FindObjectOfType<SceneHandler>().SetSpawnPoints(characterDestination.bounds, cameraDestination.position);
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += OnSceneLoaded;
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= OnSceneLoaded;
  }

  private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
  {
    if (setSpawnPointsOnLoad) PersistSpawnPoints();
  }

  private bool CheckDistance(Movement movable, Vector2 destination)
  {
    if (movable == null) return true;

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

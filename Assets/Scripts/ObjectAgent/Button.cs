using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class Button : MonoBehaviour, IInteractable
{
    [SerializeField] private Door _linkedDoor;
    [SerializeField] private Canvas _interactionCanvas;
    //[SerializeField] private NavMeshSurface _navMeshSurface;

    private bool _playerInRange = false;

    void Start()
    {
        if (_interactionCanvas != null)
            _interactionCanvas.enabled = false;
    }

    void Update()
    {
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void ShowUI(bool show)
    {
        if (_interactionCanvas != null)
            _interactionCanvas.enabled = show;
    }

    public void Interact()
    {
        if (_linkedDoor != null)
        {
            _linkedDoor.ToggleDoor();
            //if (_navMeshSurface != null)
            //    _navMeshSurface.BuildNavMesh();   ho usato NavMeshObstacle (carve ON) sulle porte, quindi non più necessario
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = true;
            ShowUI(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInRange = false;
            ShowUI(false);
        }
    }
}
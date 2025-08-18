using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgentBehaviour : MonoBehaviour
{
    public static PlayerAgentBehaviour Instance;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            other.GetComponent<IInteractable>()?.ShowUI(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Interactable"))
        {
            other.GetComponent<IInteractable>()?.ShowUI(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, 2f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Interactable")) hit.GetComponent<IInteractable>()?.Interact();
            }
        }
    }
}

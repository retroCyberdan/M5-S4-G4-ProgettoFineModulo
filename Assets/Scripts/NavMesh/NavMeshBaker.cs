using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    [SerializeField] private GameObject[] _wallsToRemove;
    [SerializeField] private GameObject[] _wallsToAdd;
    [SerializeField] private NavMeshSurface[] _navMeshToUpgrade;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            for (int i = 0; i < _wallsToRemove.Length; ++i)
            {
                _wallsToRemove[i].SetActive(false);
            }
            for (int i = 0; i < _wallsToAdd.Length; ++i)
            {
                _wallsToAdd[i].SetActive(true);
            }
            for (int i = 0; i < _navMeshToUpgrade.Length; i++)
            {
                _navMeshToUpgrade[i].UpdateNavMesh(_navMeshToUpgrade[i].navMeshData);
            }
            Destroy(gameObject);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentIdleRotation : MonoBehaviour
{
    [Header("Rotation Prameters")]
    [SerializeField] private float _rotationInterval = 3f;
    [SerializeField] private float _rotationSpeed = 90f;
    [SerializeField] private bool _randomRotation = true;

    private NavMeshAgent _agent;
    private Quaternion _targetRotation;

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();

        _agent.speed = 0f;
        _agent.updateRotation = false;

        StartCoroutine(RotateRoutine());
    }

    private IEnumerator RotateRoutine()
    {
        while (true)
        {
            float newYRotation = _randomRotation ? Random.Range(0f, 360f) : transform.eulerAngles.y + 90f; // <- rotazione in base alla scelta

            _targetRotation = Quaternion.Euler(0f, newYRotation, 0f);

            while (Quaternion.Angle(transform.rotation, _targetRotation) > 0.1f) // <- ruota finché non raggiunge la nuova rotazione
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(_rotationInterval); // <- aspetta prima della prossima rotazione
        }
    }
}

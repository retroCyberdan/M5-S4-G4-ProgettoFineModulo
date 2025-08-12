using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AGENT_STATUS { PATROL, ALLERT }

public class AgentPatrollingPath : MonoBehaviour
{
    [Header("Patrol Parameters")]
    [SerializeField] private Transform[] _destinationWaypoints;
    [SerializeField] private float _radius = 5f;
    [SerializeField] private LayerMask _patrolLayerMask;
    [SerializeField] private float _patrollingWaitingTime = 1.5f;

    private int _destinationWaypointIndex;
    private NavMeshAgent _agent;
    private AGENT_STATUS _currentStatus = AGENT_STATUS.PATROL;
    private Transform _playerPosition;
    private bool _isWaiting = false;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        SetDestinationAtIndex(0);
    }

    // Update is called once per frame
    void Update()
    {
        CheckAgentStatus();

        if (IsPlayerNear()) _currentStatus = AGENT_STATUS.ALLERT;

        else _currentStatus = AGENT_STATUS.PATROL;
    }

    public void SetDestinationAtNextIndex() => SetDestinationAtIndex(_destinationWaypointIndex + 1);
    public void SetDestinationAtPreviousIndex() => SetDestinationAtIndex(_destinationWaypointIndex - 1);
    public void SetDestinationAtIndex(int index) // <- per settare la destinazione
    {
        if (index < 0)
        {
            while (index < 0) index += _destinationWaypoints.Length;
        }

        else if (index >= _destinationWaypoints.Length) index = index % _destinationWaypoints.Length;

        _destinationWaypointIndex = index;

        _agent.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
    }

    public bool IsCloseEnoughToDestination() // <- per controllare quando è abbastanza vicino al waypoint
    {
        float sqrStoppingDistance = _agent.stoppingDistance * _agent.stoppingDistance;
        Vector3 toDestination = _destinationWaypoints[_destinationWaypointIndex].position - transform.position;
        toDestination.y = 0; // <- fondamentale altrimenti l`agent sarà sempre lontano di circa 1 dalla destinazione e non raggiungerà mai la prossima destinazione (colpa del transform.position)
        float sqrDistance = toDestination.sqrMagnitude;

        if (sqrDistance <= sqrStoppingDistance + 0.1f) return true; // <- con Mathf.Epsilon non funzionava -_-

        _agent.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
        return false;
    }

    private void CheckAgentStatus() // < per scegliere se pattugliare o inseguire il player
    {
        switch (_currentStatus)
        {
            case AGENT_STATUS.PATROL:
                StartPatrol();
                break;
            case AGENT_STATUS.ALLERT:
                StartChase();
                break;
        }
    }

    private void StartPatrol() // <- per gestire il pattugliamento
    {
        if (_isWaiting) return;
        
        if (IsCloseEnoughToDestination())
        {
            SetDestinationAtNextIndex();
            StartCoroutine(PatrollingStopTime());
        }
    }

    IEnumerator PatrollingStopTime() // <- coroutine per gestire l`attesa tra un movimento e l`altro
    {
        _isWaiting = true;
        _agent.isStopped = true;
        yield return new WaitForSeconds(_patrollingWaitingTime);
        _agent.isStopped = false;
        _isWaiting = false;
    }

    private void StartChase() // <- per iniziare a seguire il player
    {
        if (_playerPosition != null)
        {
            _agent.SetDestination(_playerPosition.position);
            _agent.isStopped = false;
        }        
    }

    private bool IsPlayerNear() // <- per valutare se il player è nella visuale dell`agente (enemy)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, _radius, _patrolLayerMask);
        if (colliders != null)
        {
            foreach (Collider collider in colliders)
            {
                if (collider.CompareTag("Player"))
                {
                    _playerPosition = collider.transform; // <- mi tengo da parte la posizione del player
                    return true;
                }
            }
        }
        return false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AGENT_STATUS { IDLE_ROTATION, PATROL, ALLERT, RETURNING }

public class EnemyAgentBehaviour : MonoBehaviour
{
    [Header("FSM Settings")]
    public AGENT_STATUS startingState = AGENT_STATUS.IDLE_ROTATION;
    [HideInInspector] public IEnemyAgentStateFSM PreviousState;
    [HideInInspector] public AGENT_STATUS LastNonAllertStatus;

    [Header("Idle Rotation Settings")]
    public float IdleRotateAngle = 90f;
    public float IdleRotateInterval = 2f;
    public float RotationSpeed = 2f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] _destinationWaypoints;
    [SerializeField] private float _patrollingWaitingTime = 1.5f;
    [SerializeField] private LayerMask _patrolLayerMask;

    [Header("FOV Settings")]
    [SerializeField] private float _radius = 5f;
    [SerializeField, Range(10f, 120f)] private float _visionAngle = 60f;
    [SerializeField] private int _visionSegments = 30;
    [SerializeField] private Color _visionColor = Color.yellow;

    [Header("Return/Arrival Settings")]
    public float arriveTolerance = 0.05f;
    public float lostChaseReturnDelay = 1.0f;

    [Header("Debug")]
    public bool debugLogs = false;

    #region FSM Runtime
    private IEnemyAgentStateFSM _currentState;
    private int _destinationWaypointIndex;
    private Coroutine _returnCoroutine;
    #endregion

    #region Components
    public NavMeshAgent AgentNavMesh { get; private set; }
    public Transform PlayerTarget { get; set; }
    private LineRenderer _lineRenderer;
    #endregion

    #region Original Position
    public Vector3 OriginalPosition { get; private set; }
    public Quaternion OriginalRotation { get; private set; }
    #endregion

    #region Properties
    public Transform[] DestinationWaypoints => _destinationWaypoints;
    public float PatrollingWaitingTime => _patrollingWaitingTime;
    public float Radius => _radius;
    public float VisionAngle => _visionAngle;
    public LayerMask PatrolLayerMask => _patrolLayerMask;
    public int DestinationWaypointIndex => _destinationWaypointIndex;
    #endregion

    private void Awake()
    {
        AgentNavMesh = GetComponent<NavMeshAgent>();
        OriginalPosition = transform.position;
        OriginalRotation = transform.rotation;
    }

    private void Start()
    {
        LineRendererSetup();
        GoToStartingState();
    }

    private void Update()
    {
        _currentState?.Update(this);

        if (_lineRenderer != null)
        {
            if (_currentState is FSM_AllertState)
            {
                _lineRenderer.startColor = Color.red;
                _lineRenderer.endColor = Color.red;
            }
            else
            {
                _lineRenderer.startColor = _visionColor;
                _lineRenderer.endColor = _visionColor;
            }
        }

        DrawFOV(_visionAngle, _radius, _visionSegments);
    }

    #region FSM
    public void ChangeState(IEnemyAgentStateFSM newState)
    {
        if (_currentState != null && !(_currentState is FSM_AllertState)) LastNonAllertStatus = GetStatusFromState(_currentState);

        if (debugLogs) Debug.Log($"[FSM] {name}: {_currentState?.GetType().Name ?? "None"} -> {newState.GetType().Name}");
        _currentState?.Exit(this);
        _currentState = newState;
        _currentState.Enter(this);
    }

    public void GoToStartingState()
    {
        switch (startingState)
        {
            case AGENT_STATUS.IDLE_ROTATION: ChangeState(new FSM_IdleRotationState()); break;
            case AGENT_STATUS.PATROL: ChangeState(new FSM_PatrolState()); break;
            case AGENT_STATUS.ALLERT: ChangeState(new FSM_AllertState()); break;
            case AGENT_STATUS.RETURNING: ChangeState(new FSM_ReturningState()); break;
        }
    }
    public void ReturnToPreviousState()
    {
        if (PreviousState != null)
        {
            ChangeState(PreviousState);
        }
    }
    private AGENT_STATUS GetStatusFromState(IEnemyAgentStateFSM state)
    {
        if (state is FSM_IdleRotationState) return AGENT_STATUS.IDLE_ROTATION;
        if (state is FSM_PatrolState) return AGENT_STATUS.PATROL;
        if (state is FSM_ReturningState) return AGENT_STATUS.RETURNING;
        return AGENT_STATUS.IDLE_ROTATION;
    }
    #endregion

    #region Waypoints
    public void SetDestinationAtIndex(int index)
    {
        if (_destinationWaypoints.Length == 0) return;
        _destinationWaypointIndex = index % _destinationWaypoints.Length;
        AgentNavMesh.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
    }

    public void SetDestinationAtNextIndex()
    {
        if (_destinationWaypoints.Length == 0) return;
        _destinationWaypointIndex = (_destinationWaypointIndex + 1) % _destinationWaypoints.Length;
        AgentNavMesh.SetDestination(_destinationWaypoints[_destinationWaypointIndex].position);
    }

    public bool HasArrivedAtDestination()
    {
        if (AgentNavMesh.pathPending) return false;
        return AgentNavMesh.remainingDistance <= AgentNavMesh.stoppingDistance + arriveTolerance;
    }

    public int GetClosestWaypointIndex()
    {
        if (_destinationWaypoints.Length == 0) return 0;

        int closestIndex = 0;
        float closestSqrDist = float.MaxValue;

        for (int i = 0; i < _destinationWaypoints.Length; i++)
        {
            Vector3 toWaypoint = _destinationWaypoints[i].position - transform.position;
            toWaypoint.y = 0f; // ignora altezza
            float sqrDist = toWaypoint.sqrMagnitude;

            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestIndex = i;
            }
        }

        return closestIndex;
    }
    #endregion

    #region FOV
    private void LineRendererSetup()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.positionCount = _visionSegments + 2; // +2 per centro e chiusura
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        _lineRenderer.startColor = _visionColor;
        _lineRenderer.endColor = _visionColor;
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;
    }

    private void DrawFOV(float angle, float radius, int segments)
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = segments + 2;
        _lineRenderer.SetPosition(0, transform.position);

        float halfAngle = angle / 2f;

        for (int i = 0; i <= segments; i++)
        {
            float currentAngle = -halfAngle + (angle / segments) * i;
            float rad = currentAngle * Mathf.Deg2Rad;

            float x = Mathf.Sin(rad) * radius;
            float z = Mathf.Cos(rad) * radius;

            Vector3 point = transform.position + transform.rotation * new Vector3(x, 0, z);
            _lineRenderer.SetPosition(i + 1, point);
        }
    }

    public bool IsPlayerNear()
    {
        if (PlayerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) PlayerTarget = player.transform;
        }

        if (PlayerTarget == null) return false;

        Vector3 dirToPlayer = (PlayerTarget.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);
        if (angleToPlayer <= _visionAngle / 2f)
        {
            float dist = Vector3.Distance(transform.position, PlayerTarget.position);
            if (dist <= _radius) return true;
        }
        return false;
    }
    #endregion

    #region Return methods
    public void StartReturnToOriginal(float delay)
    {
        if (_returnCoroutine != null) StopCoroutine(_returnCoroutine);
        _returnCoroutine = StartCoroutine(ReturnCoroutine(delay));
    }

    private IEnumerator ReturnCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(new FSM_ReturningState());
        _returnCoroutine = null;
    }
    #endregion

    #region Misc
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.OnPlayerCaught(other.gameObject);
        }
    }
    #endregion
}
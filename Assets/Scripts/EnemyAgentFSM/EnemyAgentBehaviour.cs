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
    [SerializeField] private Color _alertColor = Color.red;
    [SerializeField] private LayerMask _obstacleLayer = -1;
    [SerializeField] private LayerMask _playerLayer = 1;

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
        DrawFOV();
        UpdateFOVColor();
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
            toWaypoint.y = 0f; // <- ignora altezza
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

    #region FOV with Obstacles detection
    private void LineRendererSetup()
    {
        _lineRenderer = gameObject.AddComponent<LineRenderer>();
        _lineRenderer.widthMultiplier = 0.05f;
        _lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        SetLineRendererColor(_visionColor);
        _lineRenderer.useWorldSpace = true;
        _lineRenderer.loop = true;
        _lineRenderer.sortingOrder = 1;
    }

    private void DrawFOV()
    {
        if (_lineRenderer == null) return;

        List<Vector3> visionPoints = CalculateFOVPoints();
        ApplyPointsToLineRenderer(visionPoints);
    }

    private List<Vector3> CalculateFOVPoints()
    {
        List<Vector3> points = new List<Vector3>();
        Vector3 origin = GetFOVOrigin();

        points.Add(origin); // <- aggiungi il punto di origine

        // calcola i parametri del cono
        float startAngle = -_visionAngle / 2f;
        float angleIncrement = _visionAngle / _visionSegments;

        // genera i punti del perimetro del cono con rilevamento ostacoli
        for (int i = 0; i <= _visionSegments; i++)
        {
            float currentAngle = startAngle + (angleIncrement * i);
            Vector3 rayDirection = GetDirectionFromAngle(currentAngle);

            float rayDistance = GetRayDistanceWithObstacles(origin, rayDirection);
            Vector3 endPoint = origin + rayDirection * rayDistance;

            points.Add(endPoint);
        }

        return points;
    }

    private Vector3 GetFOVOrigin()
    {
        return transform.position + Vector3.up * 0.1f; // <- posizione leggermente elevata per evitare problemi con il terreno (come ha suggerito il prof. Luca)
    }

    private Vector3 GetDirectionFromAngle(float angleOffset)
    {
        float totalAngle = transform.eulerAngles.y + angleOffset;
        float radians = totalAngle * Mathf.Deg2Rad;

        return new Vector3(Mathf.Sin(radians), 0, Mathf.Cos(radians));
    }

    private float GetRayDistanceWithObstacles(Vector3 origin, Vector3 direction)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, _radius, _obstacleLayer))
        {
            return hit.distance;
        }
        return _radius;
    }

    private void ApplyPointsToLineRenderer(List<Vector3> points)
    {
        _lineRenderer.positionCount = points.Count;
        _lineRenderer.SetPositions(points.ToArray());
    }

    private void UpdateFOVColor()
    {
        Color targetColor = (_currentState is FSM_AllertState) ? _alertColor : _visionColor;
        SetLineRendererColor(targetColor);
    }

    private void SetLineRendererColor(Color color)
    {
        // crea un gradiente uniforme con il colore che gli passo
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(color.a, 0.0f), new GradientAlphaKey(color.a, 1.0f) }
        );
        _lineRenderer.colorGradient = gradient;
    }

    public bool IsPlayerNear()
    {
        if (PlayerTarget == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) PlayerTarget = player.transform;
        }

        if (PlayerTarget == null) return false;

        return IsPlayerInFOV();
    }

    private bool IsPlayerInFOV()
    {
        Vector3 origin = GetFOVOrigin();
        Vector3 toPlayer = PlayerTarget.position - origin;
        float distanceToPlayer = toPlayer.magnitude;

        // controllo distanza
        if (distanceToPlayer > _radius) return false;

        // controllo angolo
        Vector3 directionToPlayer = toPlayer.normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
        if (angleToPlayer > _visionAngle / 2f) return false;

        // controllo ostacoli + verifica che non ci siano ostacoli tra nemico e player
        return !IsPlayerBlockedByObstacle(origin, directionToPlayer, distanceToPlayer);
    }

    private bool IsPlayerBlockedByObstacle(Vector3 origin, Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, distance, _obstacleLayer))
        {
            return true;
        }

        // Doppio controllo con raycast verso il player specifico
        if (Physics.Raycast(origin, direction, out hit, distance))
        {
            return hit.transform != PlayerTarget;
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

    #region Debug Gizmos
    private void OnDrawGizmosSelected()
    {
        // disegna il raggio di visione
        Gizmos.color = (_currentState is FSM_AllertState) ? Color.red : Color.white;
        Gizmos.DrawWireSphere(transform.position, _radius);

        // disegna i confini dell'angolo di visione
        Vector3 leftBoundary = GetDirectionFromAngle(-_visionAngle / 2f) * _radius;
        Vector3 rightBoundary = GetDirectionFromAngle(_visionAngle / 2f) * _radius;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        // mostra direzione verso il player qualora rilevato
        if (IsPlayerNear() && PlayerTarget != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, PlayerTarget.position);
        }
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
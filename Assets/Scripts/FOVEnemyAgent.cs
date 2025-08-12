using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR;

//
// script per movimento verso il player di un agent all`interno di una NavMesh creato dal professore Luca Villaini
//

public enum EnemyState
{
    Patrol,
    Chase
}

//public class FOVEnemyAgent : MonoBehaviour
//{
//    [SerializeField] WaypointsManager waypointsManager;
//    [SerializeField] float waitTime = 3f;
//    [SerializeField] EnemyState currentState = EnemyState.Patrol;

//    NavMeshAgent agent;
//    FieldOfView fov;
//    Transform[] waypoints;
//    int currentIndex = 0;
//    bool isWaiting;
//    Coroutine waitingCoroutine;

//    void Awake()
//    {
//        agent = GetComponent<NavMeshAgent>();
//        fov = GetComponent<FieldOfView>();
//    }

//    // Start is called before the first frame update
//    void Start()
//    {
//        waypoints = waypointsManager.GetWaypoints();

//        agent.SetDestination(waypoints[currentIndex].position);
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (fov.IsPlayerInRange())
//        {
//            ChangeState(EnemyState.Chase);
//        }
//        else
//        {
//            ChangeState(EnemyState.Patrol);
//        }

//        switch (currentState)
//        {
//            case EnemyState.Patrol:
//                Patrolling();
//                break;
//            case EnemyState.Chase:
//                Chasing();
//                break;
//        }
//    }

//    private void Chasing()
//    {
//        if (agent.isStopped)
//            agent.isStopped = false;

//        if (waitingCoroutine != null)
//        {
//            isWaiting = false;
//            StopCoroutine(waitingCoroutine);
//        }

//        agent.SetDestination(fov.Player.position);
//    }

//    private void Patrolling()
//    {
//        if (isWaiting)
//            return;

//        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
//        {
//            //currentIndex = (currentIndex + 1) % waypoints.Length;
//            //agent.SetDestination(waypoints[currentIndex].position);

//            waitingCoroutine = StartCoroutine(WaitInternal());
//        }
//    }

//    IEnumerator WaitInternal()
//    {
//        isWaiting = true;
//        agent.isStopped = true;
//        yield return new WaitForSeconds(waitTime);

//        currentIndex = (currentIndex + 1) % waypoints.Length;
//        agent.SetDestination(waypoints[currentIndex].position);

//        agent.isStopped = false;
//        isWaiting = false;
//    }

//    private void ChangeState(EnemyState newState)
//    {
//        currentState = newState;
//    }
//}
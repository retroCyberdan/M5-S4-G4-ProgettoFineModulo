using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FSM_PatrolState : IEnemyAgentStateFSM
{
    private bool _isWaiting = false;

    public void Enter(EnemyAgentBehaviour agent)
    {
        if (agent.DestinationWaypoints.Length > 0) agent.SetDestinationAtIndex(0);
        agent.AgentNavMesh.isStopped = false;
    }

    public void Update(EnemyAgentBehaviour agent)
    {
        if (agent.IsPlayerNear())
        {
            agent.ChangeState(new FSM_AllertState());
            return;
        }

        if (!_isWaiting && agent.HasArrivedAtDestination())
        {
            agent.SetDestinationAtNextIndex();
            agent.StartCoroutine(PatrollingWait(agent));
        }
    }

    public void Exit(EnemyAgentBehaviour agent)
    {
        _isWaiting = false;
    }

    private IEnumerator PatrollingWait(EnemyAgentBehaviour agent)
    {
        _isWaiting = true;
        agent.AgentNavMesh.isStopped = true;
        yield return new WaitForSeconds(agent.PatrollingWaitingTime);
        agent.AgentNavMesh.isStopped = false;
        _isWaiting = false;
    }
}
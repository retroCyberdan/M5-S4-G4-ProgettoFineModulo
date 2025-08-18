using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_ReturningState : IEnemyAgentStateFSM
{
    public void Enter(EnemyAgentBehaviour agent)
    {
        agent.AgentNavMesh.isStopped = false;
        agent.AgentNavMesh.SetDestination(agent.OriginalPosition);
    }

    public void Update(EnemyAgentBehaviour agent)
    {
        if (agent.HasArrivedAtDestination())
        {
            agent.AgentNavMesh.isStopped = true;
            agent.transform.rotation = agent.OriginalRotation;
            int closestIndex = agent.GetClosestWaypointIndex();
            agent.SetDestinationAtIndex(closestIndex);
            agent.ChangeState(new FSM_IdleRotationState());
        }
    }

    public void Exit(EnemyAgentBehaviour agent) { }
}
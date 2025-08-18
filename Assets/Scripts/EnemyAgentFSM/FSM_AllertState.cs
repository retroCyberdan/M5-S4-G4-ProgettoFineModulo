using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_AllertState : IEnemyAgentStateFSM
{
    public void Enter(EnemyAgentBehaviour agent)
    {
        agent.AgentNavMesh.isStopped = false;
    }

    public void Update(EnemyAgentBehaviour agent)
    {
        if (agent.IsPlayerNear())
        {
            agent.AgentNavMesh.isStopped = false;
            agent.AgentNavMesh.SetDestination(agent.PlayerTarget.position);
        }
        else
        {
            agent.PlayerTarget = null;

            switch (agent.LastNonAllertStatus)
            {
                case AGENT_STATUS.IDLE_ROTATION:
                    agent.ChangeState(new FSM_IdleRotationState());
                    break;
                case AGENT_STATUS.PATROL:
                    int closest = agent.GetClosestWaypointIndex();
                    agent.SetDestinationAtIndex(closest);
                    agent.ChangeState(new FSM_PatrolState());
                    break;
                case AGENT_STATUS.RETURNING:
                    agent.ChangeState(new FSM_ReturningState());
                    break;
            }
        }
    }

    public void Exit(EnemyAgentBehaviour agent) { }
}
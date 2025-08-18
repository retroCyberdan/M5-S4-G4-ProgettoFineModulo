using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM_IdleRotationState : IEnemyAgentStateFSM
{
    private Coroutine _idleCoroutine;
    private bool _isRotating = false;
    private bool _returningToOriginal = false;

    public void Enter(EnemyAgentBehaviour agent)
    {
        if ((agent.transform.position - agent.OriginalPosition).sqrMagnitude > 0.01f)
        {
            _returningToOriginal = true;
            agent.AgentNavMesh.isStopped = false;
            agent.AgentNavMesh.SetDestination(agent.OriginalPosition);
        }
        else
        {
            StartIdleRotation(agent);
        }
    }

    public void Update(EnemyAgentBehaviour agent)
    {
        if (agent.IsPlayerNear())
        {
            if (_idleCoroutine != null)
            {
                agent.StopCoroutine(_idleCoroutine);
                _idleCoroutine = null;
                _isRotating = false;
                _returningToOriginal = false;
            }
            agent.ChangeState(new FSM_AllertState());
        }

        if (_returningToOriginal)
        {
            if (!agent.AgentNavMesh.pathPending && agent.AgentNavMesh.remainingDistance <= agent.AgentNavMesh.stoppingDistance + 0.05f)
            {
                _returningToOriginal = false;
                StartIdleRotation(agent);
            }
        }
    }

    public void Exit(EnemyAgentBehaviour agent)
    {
        if (_idleCoroutine != null)
        {
            agent.StopCoroutine(_idleCoroutine);
            _idleCoroutine = null;
            _isRotating = false;
        }
        _returningToOriginal = false;
    }
    private void StartIdleRotation(EnemyAgentBehaviour agent)
    {
        agent.AgentNavMesh.isStopped = true;
        _idleCoroutine = agent.StartCoroutine(IdleRotation(agent));
    }

    private IEnumerator IdleRotation(EnemyAgentBehaviour agent)
    {
        _isRotating = true;

        while (_isRotating)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, agent.transform.eulerAngles.y + agent.IdleRotateAngle, 0f);
            float elapsed = 0f;

            while (elapsed < agent.IdleRotateInterval)
            {
                agent.transform.rotation = Quaternion.Lerp(agent.transform.rotation, targetRotation, Time.deltaTime * agent.RotationSpeed);
                elapsed += Time.deltaTime;
                yield return null;
            }

            agent.transform.rotation = targetRotation;
            yield return null;
        }
    }
}
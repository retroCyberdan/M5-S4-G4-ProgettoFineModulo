using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemyAgentStateFSM
{
    void Enter(EnemyAgentBehaviour agent);
    void Update(EnemyAgentBehaviour agent);
    void Exit(EnemyAgentBehaviour agent);
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    [SerializeField] private float _stopThreshold = 0.05f;

    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float speed = _agent.velocity.magnitude;

        if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance) speed = 0f;

        _animator.SetFloat("speed", speed);
    }
}

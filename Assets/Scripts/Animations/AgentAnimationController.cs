using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentAnimationController : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;
    
    private Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        _animator.SetFloat("speed", _agent.velocity.magnitude);
    }
}

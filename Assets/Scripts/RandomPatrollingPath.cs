using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

//
// script per movimento randomico di un agent all`interno di una NavMesh creato dal professore Luca Villaini
//
public class RandomPatrollingPath : MonoBehaviour
{
    [SerializeField] float radius = 10f;
    [SerializeField] float nextSearchTimer = 5f;

    NavMeshAgent agent;
    float timer;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        timer = nextSearchTimer;
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= nextSearchTimer)
        {
            Vector3 nextPos = RandomPosition(transform.position, radius, -1);
            agent.SetDestination(nextPos);
            timer = 0f;
        }
    }

    private Vector3 RandomPosition(Vector3 position, float radius, int layerMask)
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += position;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, radius, layerMask);
        return hit.position;
    }
}

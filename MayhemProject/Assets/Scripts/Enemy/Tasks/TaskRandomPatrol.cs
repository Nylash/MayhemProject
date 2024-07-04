using BehaviourTree;
using UnityEngine;
using UnityEngine.AI;
using static Utilities;

public class TaskRandomPatrol : Node
{
    private NavMeshAgent _agent;
    private Vector2 _randomDistances;
    private float _stoppingDistance;
    private Vector3 _patrolPoint;

    public TaskRandomPatrol(NavMeshAgent agent, Vector2 randomDistances, float stoppingDistance) : base()
    {
        _agent = agent;
        _randomDistances = randomDistances;
        _stoppingDistance = stoppingDistance;

        _patrolPoint = GetRandomPointOnNavMesh(_randomDistances.x, _randomDistances.y, 10);
    }

    public override NodeState Evaluate()
    {
        if(DistanceOnXZ(_agent.transform.position, _patrolPoint) < _stoppingDistance)
        {
            _patrolPoint = GetRandomPointOnNavMesh(_randomDistances.x, _randomDistances.y, 10);
        }
        if(_agent.destination != _patrolPoint)
        {
            _agent.SetDestination(_patrolPoint);
        }

        state = NodeState.SUCCESS;
        return state;
    }

    private Vector3 GetRandomPointOnNavMesh(float minDist, float maxDist, int attempts)
    {
        for (int i = 0; i < attempts; i++)
        {
            Vector2 randomDirection2D = Random.insideUnitCircle * maxDist;
            Vector3 randomDirection = new Vector3(randomDirection2D.x, 0, randomDirection2D.y);
            randomDirection += _agent.transform.position;
            NavMeshHit navHit;

            if (NavMesh.SamplePosition(randomDirection, out navHit, maxDist, -1))
            {
                float distance = Vector3.Distance(_agent.transform.position, navHit.position);
                if (distance >= minDist && distance <= maxDist)
                {
                    if (IsPointValid(navHit.position))
                    {
                        DebugExtension.DrawSphere(navHit.position, .1f, Color.red, 1);
                        return navHit.position;
                    }
                }
            }
        }
        Debug.LogError("TaskRandomPatrol : Couldn't find random point on navMesh for " + _agent.name);
        return _agent.transform.position; // Si aucune position valide n'est trouvée après les tentatives
    }

    bool IsPointValid(Vector3 point)
    {
        NavMeshPath path = new NavMeshPath();
        _agent.CalculatePath(point, path);
        Debug.LogWarning(path.status == NavMeshPathStatus.PathComplete);
        return path.status == NavMeshPathStatus.PathComplete;
    }
}

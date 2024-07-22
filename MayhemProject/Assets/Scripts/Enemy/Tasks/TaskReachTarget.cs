using BehaviourTree;
using UnityEngine.AI;
using UnityEngine;

public class TaskReachTarget : Node
{
    private NavMeshAgent _navMeshAgent;

    public TaskReachTarget(NavMeshAgent navMeshAgent) : base()
    {
        _navMeshAgent = navMeshAgent;
    }

    public override NodeState Evaluate()
    {
        if (_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = false;
        }

        _navMeshAgent.SetDestination((Root.GetData("Target") as Transform).position);

        state = NodeState.RUNNING;
        return state;
    }
}

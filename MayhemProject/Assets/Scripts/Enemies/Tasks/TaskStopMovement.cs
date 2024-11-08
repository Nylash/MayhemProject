using BehaviourTree;
using UnityEngine.AI;

public class TaskStopMovement : Node
{
    private NavMeshAgent _navMeshAgent;

    public TaskStopMovement(NavMeshAgent agent)
    {
        _navMeshAgent = agent;
    }

    public override NodeState Evaluate()
    {
        if (!_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = true;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}

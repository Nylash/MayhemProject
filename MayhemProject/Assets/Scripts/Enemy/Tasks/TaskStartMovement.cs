using BehaviourTree;
using UnityEngine.AI;

public class TaskStartMovement : Node
{
    private NavMeshAgent _navMeshAgent;

    public TaskStartMovement(NavMeshAgent agent)
    {
        _navMeshAgent = agent;
    }

    public override NodeState Evaluate()
    {
        if (_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = false;
        }

        state = NodeState.SUCCESS;
        return state;
    }
}

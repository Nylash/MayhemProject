using BehaviourTree;
using UnityEngine.AI;

public class TaskAttackTarget : Node
{
    private NavMeshAgent _navMeshAgent;

    public TaskAttackTarget(NavMeshAgent agent)
    {
        _navMeshAgent = agent;
    }

    public override NodeState Evaluate()
    {
        if (!_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = true;
        }

        //FIRE !

        state = NodeState.SUCCESS;
        return state;
    }
}

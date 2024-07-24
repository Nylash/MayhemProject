using BehaviourTree;
using System.Transactions;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TaskAttackTarget : Node
{
    private NavMeshAgent _navMeshAgent;
    private BasicEnemy_BT _behaviourTree;
    private Transform _transform;

    public TaskAttackTarget(NavMeshAgent agent, BasicEnemy_BT behaviourTree, Transform transform)
    {
        _navMeshAgent = agent;
        _behaviourTree = behaviourTree;
        _transform = transform;
    }

    public override NodeState Evaluate()
    {
        if (!_navMeshAgent.isStopped)
        {
            _navMeshAgent.isStopped = true;
        }

        _transform.LookAt(Root.GetData("Target") as Transform);

        _behaviourTree.Attack();

        state = NodeState.SUCCESS;
        return state;
    }
}

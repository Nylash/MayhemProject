using BehaviourTree;
using UnityEngine;

public class TaskAttackTarget : Node
{
    private BasicEnemy_BT _behaviourTree;
    private Transform _transform;

    public TaskAttackTarget(BasicEnemy_BT behaviourTree, Transform transform)
    {
        _behaviourTree = behaviourTree;
        _transform = transform;
    }

    public override NodeState Evaluate()
    {
        _transform.LookAt(Root.GetData("Target") as Transform);

        _behaviourTree.Attack();

        state = NodeState.RUNNING;
        return state;
    }
}

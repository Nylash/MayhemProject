using BehaviourTree;
using UnityEngine;

public class TaskAttackTarget : Node
{
    private BasicEnemy_BT _behaviourTree;
    private Transform _transform;
    private Data_Weapon _weapon;

    public TaskAttackTarget(BasicEnemy_BT behaviourTree, Transform transform, Data_Weapon weapon)
    {
        _behaviourTree = behaviourTree;
        _transform = transform;
        _weapon = weapon;
    }

    public override NodeState Evaluate()
    {
        _transform.LookAt(Root.GetData("Target") as Transform);

        _behaviourTree.Attack(_weapon);

        state = NodeState.RUNNING;
        return state;
    }
}

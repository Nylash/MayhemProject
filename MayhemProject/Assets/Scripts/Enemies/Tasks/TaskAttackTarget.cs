using BehaviourTree;

public class TaskAttackTarget : Node
{
    private BasicEnemy_BT _behaviourTree;
    private Data_Weapon _weapon;

    public TaskAttackTarget(BasicEnemy_BT behaviourTree, Data_Weapon weapon)
    {
        _behaviourTree = behaviourTree;
        _weapon = weapon;
    }

    public override NodeState Evaluate()
    {
        _behaviourTree.Attack(_weapon);

        state = NodeState.RUNNING;
        return state;
    }
}

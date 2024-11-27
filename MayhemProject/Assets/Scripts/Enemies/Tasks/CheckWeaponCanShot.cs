using BehaviourTree;

public class CheckWeaponCanShot : Node
{
    private Data_Weapon _weapon;
    private BasicEnemy_BT _behaviourTree;

    public CheckWeaponCanShot(BasicEnemy_BT behaviourTree ,Data_Weapon weapon)
    {
        _behaviourTree = behaviourTree;
        _weapon = weapon;
    }

    public override NodeState Evaluate()
    {
        if (_behaviourTree.CanShot(_weapon))
        {
            return NodeState.SUCCESS;
        }
        else
        {
            return NodeState.FAILURE;
        }
    }
}

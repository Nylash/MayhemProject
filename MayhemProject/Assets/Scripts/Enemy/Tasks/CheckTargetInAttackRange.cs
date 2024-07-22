using BehaviourTree;

public class CheckTargetInAttackRange : Node
{
    private TriggerDetection _attackTrigger;

    public CheckTargetInAttackRange(TriggerDetection attackTrigger)
    {
        _attackTrigger = attackTrigger;
    }

    public override NodeState Evaluate()
    {
        if (_attackTrigger.IsTriggered)
        {
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}

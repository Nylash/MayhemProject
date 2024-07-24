using BehaviourTree;

public class CheckTargetInAttackRange : Node
{
    private TriggerDetection _attackTrigger;

    public CheckTargetInAttackRange(TriggerDetection attackTrigger)
    {
        _attackTrigger = attackTrigger;
    }

    //Simply check if the trigger is triggered.
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

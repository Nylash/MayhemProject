using BehaviourTree;

public class CheckTargetInTrigger : Node
{
    private TriggerDetection _trigger;
    public CheckTargetInTrigger(TriggerDetection trigger)
    {
        _trigger = trigger;
    }

    //Simply check if the trigger is triggered.
    public override NodeState Evaluate()
    {
        if (_trigger.IsTriggered)
        {
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}

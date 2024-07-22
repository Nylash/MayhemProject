using BehaviourTree;

public class CheckTargetDetected : Node
{
    private TriggerDetection _detectionTrigger;

    public CheckTargetDetected(TriggerDetection detectionTrigger)
    {
        _detectionTrigger = detectionTrigger;
    }

    public override NodeState Evaluate()
    {
        if(Root.GetData("Target") != null)
        {
            state = NodeState.SUCCESS;
            return state;
        }
        if (_detectionTrigger.IsTriggered)
        {
            Root.SetData("Target", PlayerHealthManager.Instance.transform);
            state = NodeState.SUCCESS;
            return state;
        }
        state = NodeState.FAILURE;
        return state;
    }
}

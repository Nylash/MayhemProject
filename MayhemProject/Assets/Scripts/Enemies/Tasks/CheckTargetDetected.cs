using BehaviourTree;

public class CheckTargetDetected : Node
{
    private TriggerDetection _detectionTrigger;

    public CheckTargetDetected(TriggerDetection detectionTrigger)
    {
        _detectionTrigger = detectionTrigger;
    }

    //If root has a target auto succes.
    //If trigger detect a target assign it to the root and succes.
    //Otherwise no target detected, failure.
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

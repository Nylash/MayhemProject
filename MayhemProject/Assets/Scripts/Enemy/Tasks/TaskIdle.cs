using BehaviourTree;

public class TaskIdle : Node
{
    public TaskIdle() : base(){}

    public override NodeState Evaluate()
    {
        state = NodeState.SUCCESS;
        return state;
    }
}

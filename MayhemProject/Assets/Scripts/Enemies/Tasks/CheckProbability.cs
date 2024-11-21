using BehaviourTree;
using UnityEngine;

public class CheckProbability : Node
{
    private int _probability;
    private int _probabilityCheck;

    public CheckProbability(int probability)
    {
        _probability = probability;
    }

    public override NodeState Evaluate()
    {
        _probabilityCheck = Random.Range(0, 100);
        if( _probabilityCheck <= _probability)
        {
            return NodeState.SUCCESS;
        }
        else
        {
            return NodeState.FAILURE;
        }
    }
}

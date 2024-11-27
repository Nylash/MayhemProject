using BehaviourTree;
using UnityEngine;

public class CheckProbability : Node
{
    private int _probability;
    private int _probabilityCheck;
    private float _evaluationInterval = 1f; // Interval in seconds
    private float _timeSinceLastCheck = 0f;

    public CheckProbability(int probability)
    {
        _probability = probability;
    }

    public override NodeState Evaluate()
    {
        _timeSinceLastCheck += Time.deltaTime;

        if (_timeSinceLastCheck >= _evaluationInterval)
        {
            _timeSinceLastCheck = 0f;

            int probabilityCheck = Random.Range(0, 101);

            if (probabilityCheck < _probability)
            {
                return NodeState.SUCCESS;
            }
            else
            {
                return NodeState.FAILURE;
            }
        }

        // Outside of the interval, return FAILURE, so SELECTOR can check following SEQUENCE
        return NodeState.FAILURE;
    }
}

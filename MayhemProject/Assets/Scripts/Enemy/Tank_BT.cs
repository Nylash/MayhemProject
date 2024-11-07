using BehaviourTree;
using System.Collections.Generic;
using UnityEngine;

public class Tank_BT : BasicEnemy_BT
{
    [SerializeField] private TriggerDetection _detectionTrigger;
    [SerializeField] private TriggerDetection _attackTrigger;
    [SerializeField] private TriggerDetection _stopMovementTrigger;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            //Attack sequence
            new Sequence(new List<Node>
            {
                new CheckTargetInTrigger(_attackTrigger),
                new TaskAttackTarget(this, transform, _weapons[0]),
                new CheckTargetInTrigger(_stopMovementTrigger),
                new TaskStopMovement(_agent)
            }),
            //Movement sequence
            new Sequence(new List<Node>
            {
                new CheckTargetDetected(_detectionTrigger),
                new TaskStartMovement(_agent),
                new TaskReachTarget(_agent)
            }),
            //Idle
            new TaskIdle()
        });

        return root;
    }

    public override void Initialize()
    {

    }
}

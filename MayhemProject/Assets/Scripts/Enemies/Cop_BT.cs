using BehaviourTree;
using System.Collections.Generic;
using UnityEngine;

public class Cop_BT : BasicEnemy_BT
{
    [Header("PATROL")]
    [SerializeField] private Vector2 _minMaxDistances;
    [SerializeField] private float _stoppingDistance;
    [Header("OTHER")]
    [SerializeField] private TriggerDetection _detectionTrigger;
    [SerializeField] private TriggerDetection _attackTrigger;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            //Attack sequence
            new Sequence(new List<Node>
            {
                new CheckTargetInTrigger(_attackTrigger),
                new TaskStopMovement(_agent),
                new TaskAttackTarget(this, _weapons[0])
            }),
            //Movement sequence
            new Sequence(new List<Node>
            {
                new CheckTargetDetected(_detectionTrigger),
                new TaskStartMovement(_agent),
                new TaskReachTarget(_agent)
            }),
            //Idle
            new TaskRandomPatrol(_agent ,_minMaxDistances, _stoppingDistance)
        });

        return root;
    }

    public override void Initialize()
    {

    }    
}

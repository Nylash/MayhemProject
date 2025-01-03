using BehaviourTree;
using System.Collections.Generic;
using UnityEngine;

public class Boss_BT : BasicEnemy_BT
{
    [SerializeField] private TriggerDetection _longRangeTrigger;
    [SerializeField] private TriggerDetection _closeRangeTrigger;
    [Tooltip("Probability (in %) to succes every second.")]
    [SerializeField][Range(0, 100)] private int _probabilityThrowable;


    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            //Close range attack
            new Sequence(new List<Node>
            {
                new CheckTargetInTrigger(_closeRangeTrigger),
                new TaskStopMovement(_agent),
                new TaskAttackTarget(this, _weapons[2])
            }),
            //Throwable attack
            new Sequence(new List<Node>
            {
                new CheckTargetInTrigger(_longRangeTrigger),
                new CheckWeaponCanShot(this, _weapons[1]),
                new CheckProbability(_probabilityThrowable),
                new TaskAttackTarget(this, _weapons[1])

            }),
            //Long range attack
            new Sequence(new List<Node>
            {
                new TaskStartMovement(_agent),
                new TaskReachTarget(_agent),
                new CheckTargetInTrigger(_longRangeTrigger),
                new TaskAttackTarget(this, _weapons[0])
            })
        });

        root.SetData("Target", PlayerHealthManager.Instance.transform);

        return root;
    }

    public override void Initialize()
    {

    }
}

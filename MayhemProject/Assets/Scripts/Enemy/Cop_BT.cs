using BehaviourTree;
using System.Collections.Generic;
using UnityEngine;

public class Cop_BT : BasicEnemy_BT
{
    [SerializeField] private Vector2 _minMaxDistances;
    [SerializeField] private float _stoppingDistance;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new TaskRandomPatrol(_agent ,_minMaxDistances, _stoppingDistance)
        });

        return root;
    }

    public override void Initialize()
    {

    }
}

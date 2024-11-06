using BehaviourTree;
using System.Collections.Generic;

public class Tank_BT : BasicEnemy_BT
{
    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            new TaskIdle()
        });

        return root;
    }

    public override void Initialize()
    {

    }

    public override void Attack(Data_Weapon weapon)
    {
        throw new System.NotImplementedException();
    }
}

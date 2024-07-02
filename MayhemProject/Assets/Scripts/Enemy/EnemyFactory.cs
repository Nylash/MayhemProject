using System;
using UnityEngine;

public class EnemyFactory
{
    public static BasicEnemy_BT CreateEnemy(string enemyType, Vector3 position)
    {
        GameObject enemyObject = null;

        switch (enemyType)
        {
            case "Cop":
                enemyObject = GameObject.Instantiate(Resources.Load("Cop")) as GameObject;
                break;
            case "Tank":
                enemyObject = GameObject.Instantiate(Resources.Load("Tank")) as GameObject;
                break;
            case "Chopper":
                enemyObject = GameObject.Instantiate(Resources.Load("Chopper")) as GameObject;
                break;
            default:
                throw new ArgumentException("Invalid enemy type");
        }

        if (enemyObject != null)
        {
            enemyObject.transform.position = position;
            BasicEnemy_BT enemy = enemyObject.GetComponent<BasicEnemy_BT>();
            enemy.Initialize();

            return enemy;
        }

        return null;
    }
}

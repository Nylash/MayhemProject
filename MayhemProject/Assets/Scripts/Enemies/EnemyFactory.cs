using System;
using UnityEngine;

public class EnemyFactory
{
    public static BasicEnemy_BT CreateEnemy(string enemyType, Vector3 position, Transform parent)
    {
        //Load prefab from resources
        GameObject enemyObject = enemyType switch
        {
            "Cop" => GameObject.Instantiate(Resources.Load("Cop"), parent) as GameObject,
            "Tank" => GameObject.Instantiate(Resources.Load("Tank"), parent) as GameObject,
            "Chopper" => GameObject.Instantiate(Resources.Load("Chopper"), parent) as GameObject,
            _ => null,
        };

        //Initialize the enemy
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

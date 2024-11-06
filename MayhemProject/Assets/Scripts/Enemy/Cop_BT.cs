using BehaviourTree;
using System.Collections;
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
                new TaskAttackTarget(this, transform, _weapons[0])
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

    public override void Attack(Data_Weapon weapon)
    {
        if (CanShot(weapon))
        {
            StartCoroutine(Fire(weapon));
        }
    }

    private IEnumerator Fire(Data_Weapon weapon)
    {
        WeaponStatus currentWeaponStatus = GetWeaponStatus(weapon);
        //Each loop create one object
        for (int i = 0; i < weapon.ObjectsByShot; i++)
        {
            currentWeaponStatus.CurrentAmmunition--;

            GameObject _currentProjectile = Instantiate(weapon.Object, transform.position, Quaternion.identity);
            ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

            float randomAngle = Random.Range(-weapon.InaccuracyAngle, weapon.InaccuracyAngle);

            Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
            _currentProjectileBehaviourRef.Direction = shootDirection.normalized;
            _currentProjectileBehaviourRef.Speed = weapon.TravelSpeed;
            _currentProjectileBehaviourRef.Range = weapon.Range;
            _currentProjectileBehaviourRef.AssociatedWeapon = weapon;
            _currentProjectile.layer = _attackLayer;

            _currentProjectile.SetActive(true);

            //Little security to avoid waiting if there is only one object to spawn
            if (weapon.ObjectsByShot > 1)
                yield return new WaitForSeconds(weapon.TimeBetweenObjectsOfOneShot);
        }
        currentWeaponStatus.IsBetweenShots = true;
        _weaponsStatus[weapon] = currentWeaponStatus;
        yield return new WaitForSeconds(weapon.FireRate);
        currentWeaponStatus.IsBetweenShots = false;
        _weaponsStatus[weapon] = currentWeaponStatus;
    }
}

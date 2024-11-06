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
    [SerializeField] private Data_Weapon _weapon;

    protected override Node SetupTree()
    {
        Node root = new Selector(new List<Node>
        {
            //Attack sequence
            new Sequence(new List<Node>
            {
                new CheckTargetInAttackRange(_attackTrigger),
                new TaskStopMovement(_agent),
                new TaskAttackTarget(this, transform)
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

    public override void Attack()
    {
        if(_attackCoroutine == null)
        {
            _attackCoroutine = StartCoroutine(Fire());
        }
    }

    private IEnumerator Fire()
    {
        //Each loop create one object
        for (int i = 0; i < _weapon.ObjectsByShot; i++)
        {
            GameObject _currentProjectile = Instantiate(_weapon.Object, transform.position, Quaternion.identity);
            ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

            float randomAngle = Random.Range(-_weapon.InaccuracyAngle, _weapon.InaccuracyAngle);

            Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
            _currentProjectileBehaviourRef.Direction = shootDirection.normalized;
            _currentProjectileBehaviourRef.Speed = _weapon.TravelSpeed;
            _currentProjectileBehaviourRef.Range = _weapon.Range;
            _currentProjectileBehaviourRef.AssociatedWeapon = _weapon;
            _currentProjectile.layer = _attackLayer;

            _currentProjectile.SetActive(true);

            //Little security to avoid waiting if there is only one object to spawn
            if (_weapon.ObjectsByShot > 1)
                yield return new WaitForSeconds(_weapon.TimeBetweenObjectsOfOneShot);
        }
        yield return new WaitForSeconds(_weapon.AttackSpeed);
        _attackCoroutine = null;
    }
}

using System;
using UnityEngine;
using BehaviourTree;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Collections.Generic;
using System.Collections;

public abstract class BasicEnemy_BT : BehaviourTree.BehaviourTree
{
    #region COMPONENTS
    [Header("COMPOSANTS")]
    [SerializeField] private Image _HPBarFill;
    [SerializeField] private GameObject _HPBar;
    [SerializeField, Layer] protected int _attackLayer;
    [SerializeField] private Vector3 _shootingOffset;
    #endregion

    #region VARIABLES
    private float _HP;
    protected NavMeshAgent _agent;
    protected Dictionary<Data_Weapon, WeaponStatus> _weaponsStatus;
    #region ACCESSORS
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private float _maxHP;
    [SerializeField] protected List<Data_Weapon> _weapons = new List<Data_Weapon>();
    #endregion

    protected override Node SetupTree()
    {
        throw new NotImplementedException();
    }

    protected override void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _weaponsStatus = new Dictionary<Data_Weapon, WeaponStatus>();

        for (int i = 0; i < _weapons.Count; i++)
        {
            _weaponsStatus[_weapons[i]] = new WeaponStatus(_weapons[i].MagazineSize);
        }

        base.Start();
        
        _HP = _maxHP;
        UpdateHPBar();
    }

    protected void LateUpdate()
    {
        //Make HP bar always face the camera
        //_HPBar.transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }

    public abstract void Initialize();

    public void Attack(Data_Weapon weapon)
    {
        if (CanShot(weapon))
        {
            StartCoroutine(Fire(weapon));
        }
    }

    private IEnumerator Fire(Data_Weapon weapon)
    {
        WeaponStatus currentWeaponStatus = GetWeaponStatus(weapon);

        //Directly put IsBetweenShots to true, to avoid simultaneous shot
        currentWeaponStatus.IsBetweenShots = true;
        _weaponsStatus[weapon] = currentWeaponStatus;

        //Each loop create one object
        for (int i = 0; i < weapon.ObjectsByBurst; i++)
        {
            currentWeaponStatus.CurrentAmmunition--;

            GameObject _currentProjectile = Instantiate(weapon.Object, transform.position + _shootingOffset, Quaternion.identity);
            ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

            float randomAngle = UnityEngine.Random.Range(-weapon.InaccuracyAngle, weapon.InaccuracyAngle);

            Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * transform.forward;
            _currentProjectileBehaviourRef.Direction = shootDirection.normalized;
            _currentProjectileBehaviourRef.AssociatedWeapon = weapon;
            _currentProjectile.layer = _attackLayer;

            _currentProjectile.SetActive(true);

            //Little security to avoid waiting if there is only one object to spawn
            if (weapon.ObjectsByBurst > 1)
                yield return new WaitForSeconds(weapon.BurstInternalIntervall);
        }
        yield return new WaitForSeconds(weapon.FireRate);
        currentWeaponStatus.IsBetweenShots = false;
        _weaponsStatus[weapon] = currentWeaponStatus;
    }

    public void TakeDamage(float damage)
    {
        _HP -= damage;
        UpdateHPBar();
        if (_HP <= 0)
        {
            Die();
        }

        //Give to unit player as target since the player damages it
        if(_root.GetData("Target") == null)
        {
            _root.SetData("Target", PlayerHealthManager.Instance.transform);
        }
    }

    private void UpdateHPBar()
    {
        _HPBarFill.fillAmount = _HP / _maxHP;
        if (_HPBarFill.fillAmount < 1)
            _HPBar.SetActive(true);
    }


    private void Die()
    {
        Destroy(gameObject);
    }

    protected bool CanShot(Data_Weapon weapon)
    {
        //Check if weapon isn't between shots
        if (GetWeaponStatus(weapon).IsBetweenShots)
        {
            return false;
        }

        //Check if weapon isn't reloading
        if (GetWeaponStatus(weapon).IsReloading)
        {
            return false;
        }

        //Check remaining ammunition
        if (GetWeaponStatus(weapon).CurrentAmmunition - weapon.ObjectsByBurst < 0)
        {
            //Directly change weapon status (to avoid several coroutine start)
            WeaponStatus currentWeaponStatus = GetWeaponStatus(weapon);
            currentWeaponStatus.IsReloading = true;
            _weaponsStatus[weapon] = currentWeaponStatus;
            StartCoroutine(Reload(weapon));
            return false;
        }
        return true;
    }

    private IEnumerator Reload(Data_Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.ReloadDuration);
        WeaponStatus currentWeaponStatus = GetWeaponStatus(weapon);
        currentWeaponStatus.CurrentAmmunition = weapon.MagazineSize;
        currentWeaponStatus.IsReloading = false;
        _weaponsStatus[weapon] = currentWeaponStatus;
    }

    protected struct WeaponStatus
    {
        public int CurrentAmmunition;
        public bool IsReloading;
        public bool IsBetweenShots;

        public WeaponStatus(int ammunition)
        {
            CurrentAmmunition = ammunition;
            IsReloading = false;
            IsBetweenShots = false;
        }
    }

    protected WeaponStatus GetWeaponStatus(Data_Weapon weapon)
    {
        if(_weaponsStatus.TryGetValue(weapon, out WeaponStatus result))
        {
            return result;
        }
        //Return default weaponStatus (should never occur)
        Debug.LogError("GetWeaponsStatus with a weapon not in the dictionnary.");
        return new WeaponStatus(0);
    }
}

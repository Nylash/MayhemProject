using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static Utilities;

/// <summary>
/// This SO is use for weapon data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Weapon")]
public class Data_Weapon : ScriptableObject
{
    #region WEAPON INFO
    [Header("Info")]
    [SerializeField] private string _weaponName;
    [SerializeField] private WeaponType _weaponType;
    #endregion

    #region COMMONS INFO
    [Header("Commons informations")]
    [SerializeField] private float _damage;
    [SerializeField] private float _attackSpeed;
    #endregion

#pragma warning disable CS0414
    [SerializeField, ReadOnly] private string hint = "Be sure to only fill fields which correspond to weapon type, let the others null (0 for numerical values)";
#pragma warning restore CS0414

    #region PROJECTILE & ZONE INFO
    [Header("Projectile & Zone informations")]
    [SerializeField] GameObject _object;
    [SerializeField] private float _range;
    [SerializeField] private float _travelSpeed;
    [SerializeField] private int _objectsByShot;
    [SerializeField] private float _timeBetweenObjectsOfOneShot;
    #endregion

    #region PROJECTILE INFO
    [Header("Projectile informations")]
    [SerializeField] private float _reloadDuration;
    [SerializeField] private int _magazineSize;
    [SerializeField] private float _inaccuracyAngle;
    #endregion

    #region ZONE INFO
    [Header("Zone informations")]
    [SerializeField] private float _zoneRadius;
    [SerializeField] private AnimationCurve _trajectory;
    #endregion

    #region MELEE INFO
    [Header("Melee informations")]
    [SerializeField] private GameObject _attackArea;
    #endregion

    #region RUNTIME VARIABLES
    [Header("Variables")]
    private int _currentAmmunition;
    private bool _isReloading;
    #endregion

    #region ACCESSORS
    public string WeaponName { get => _weaponName; }
    public WeaponType WeaponType { get => _weaponType; }
    public float Damage { get => _damage; }
    public float AttackSpeed { get => _attackSpeed; }
    public GameObject Object { get => _object; }
    public float Range { get => _range; }
    public float TravelSpeed { get => _travelSpeed; }
    public int ObjectsByShot { get => _objectsByShot; }
    public float TimeBetweenObjectsOfOneShot { get => _timeBetweenObjectsOfOneShot; }
    public float ReloadDuration { get => _reloadDuration; }
    public int MagazineSize { get => _magazineSize; }
    public float InaccuracyAngle { get => _inaccuracyAngle; }
    public float ZoneRadius { get => _zoneRadius; }
    public GameObject AttackArea { get => _attackArea; }
    public int CurrentAmmunition { get => _currentAmmunition; 
        set 
        {
            _currentAmmunition = value;
            event_currentAmmunitionUpdated.Invoke(_currentAmmunition);
        }
    }
    public bool IsReloading { get => _isReloading; 
        set
        {
            _isReloading = value;
            event_weapondIsReloadingUpdated.Invoke(_isReloading);
        }
    }
    public AnimationCurve Trajectory { get => _trajectory; }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<bool> event_weapondIsReloadingUpdated;
    [HideInInspector] public UnityEvent<int> event_currentAmmunitionUpdated;
    #endregion

    #region METHODS
    private void OnEnable()
    {
        if (event_weapondIsReloadingUpdated == null)
            event_weapondIsReloadingUpdated = new UnityEvent<bool>();
        if (event_currentAmmunitionUpdated == null)
            event_currentAmmunitionUpdated = new UnityEvent<int>();
    }

    public IEnumerator Reload()
    {
        IsReloading = true;
        yield return new WaitForSeconds(_reloadDuration);
        CurrentAmmunition = _magazineSize;
        IsReloading = false;
    }

    public void InitializeWeapon()
    {
        CurrentAmmunition = _magazineSize;
        IsReloading = false;
    }
    #endregion
}

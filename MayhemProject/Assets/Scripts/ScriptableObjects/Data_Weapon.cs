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
    [SerializeField] private Sprite _weaponImage;
    [SerializeField] private float _weaponImageSize = 1f;
    #endregion

    #region COMMONS INFO
    [Header("Commons informations")]
    [SerializeField] private float _damage;
    [SerializeField] private float _fireRate;
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
    [SerializeField] private float _reloadDuration;
    [SerializeField] private int _magazineSize;
    #endregion

    #region PROJECTILE INFO
    [Header("Projectile informations")]
    [SerializeField] private float _inaccuracyAngle;
    [SerializeField] private float _recoil;
    #endregion

    #region ZONE INFO
    [Header("Zone informations")]
    [SerializeField] private float _zoneRadius;
    [SerializeField] private ZonePattern _pattern;
    [SerializeField] private float _distanceBetweenZones;
    [SerializeField] private AnimationCurve _trajectory;
    [SerializeField] private AnimationCurve _explosionSpeed;
    #endregion

    #region RUNTIME VARIABLES
    [Header("Variables")]
    //Be sure to initiliaze those values in InitializeWeapon to avoid wrong starting values
    private int _currentAmmunition;
    private bool _isReloading;
    private bool _isBetweenShots;
    #endregion

    #region ACCESSORS
    public string WeaponName { get => _weaponName; }
    public WeaponType WeaponType { get => _weaponType; }
    public float Damage { get => _damage; }
    public float FireRate { get => _fireRate; }
    public GameObject Object { get => _object; }
    public float Range { get => _range; }
    public float TravelSpeed { get => _travelSpeed; }
    public int ObjectsByShot { get => _objectsByShot; }
    public float TimeBetweenObjectsOfOneShot { get => _timeBetweenObjectsOfOneShot; }
    public float ReloadDuration { get => _reloadDuration; }
    public int MagazineSize { get => _magazineSize; }
    public float InaccuracyAngle { get => _inaccuracyAngle; }
    public float ZoneRadius { get => _zoneRadius; }
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
    public AnimationCurve ExplosionSpeed { get => _explosionSpeed; }
    public float DistanceBetweenZones { get => _distanceBetweenZones; }
    public ZonePattern Pattern { get => _pattern; }
    public Sprite WeaponImage { get => _weaponImage; }
    public float WeaponImageSize { get => _weaponImageSize; }
    public float Recoil { get => _recoil; }
    public bool IsBetweenShots { get => _isBetweenShots; set => _isBetweenShots = value; }
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
        IsBetweenShots = false;
    }

    public bool CanBeReloaded()
    {
        if(_weaponType != WeaponType.PROJECTILE)
        {
            return false;
        }
        if (_isReloading)
        {
            return false;
        }
        if (_currentAmmunition == _magazineSize)
        {
            return false;
        }
        return true;
    }
    #endregion
}

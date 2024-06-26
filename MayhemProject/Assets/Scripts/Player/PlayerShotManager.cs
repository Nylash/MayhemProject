using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static Utilities;

public class PlayerShotManager : Singleton<PlayerShotManager>
{
    #region COMPONENTS
    private ControlsMap _controlsMap;
    #endregion

    #region VARIABLES
    private Coroutine _primaryShotCoroutine;
    private Coroutine _secondaryShotCoroutine;
    private bool _stopPrimaryShotCoroutine;
    private bool _stopSecondaryShotCoroutine;
    private Data_Weapon _currentWeaponUsed;
    #region ACCESSORS

    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField, Layer] private int _attackLayer;
    #endregion

    #region EVENTS
    private UnityEvent<Data_Weapon, bool> event_primaryShotInputEnded;
    private UnityEvent<Data_Weapon, bool> event_secondaryShotInputEnded;
    #endregion

    private void OnEnable()
    {
        _controlsMap.Gameplay.Enable();
        if (event_primaryShotInputEnded == null)
            event_primaryShotInputEnded = new UnityEvent<Data_Weapon, bool>();
        if (event_secondaryShotInputEnded == null)
            event_secondaryShotInputEnded = new UnityEvent<Data_Weapon, bool>();
    } 
    private void OnDisable() => _controlsMap.Gameplay.Disable();

    protected override void OnAwake()
    {
        _controlsMap = new ControlsMap();

        _controlsMap.Gameplay.PrimaryShot.performed += ctx => StartPrimaryShot();
        _controlsMap.Gameplay.PrimaryShot.canceled += ctx => StopPrimaryShot();
        _controlsMap.Gameplay.SecondaryShot.performed += ctx => StartSecondaryShot();
        _controlsMap.Gameplay.SecondaryShot.canceled += ctx => StopSecondaryShot();
        _controlsMap.Gameplay.Reload.performed += ctx => Reload();
    }

    private void Start()
    {
        _characterData.PrimaryWeapon.InitializeWeapon();
        _characterData.SecondaryWeapon.InitializeWeapon();

        _currentWeaponUsed = _characterData.PrimaryWeapon;

        //Subscribe to the event if the weapon is of type ZONE
        if(_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            event_primaryShotInputEnded.AddListener(StartZoneWeaponShooting);
        }
        if(_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            event_secondaryShotInputEnded.AddListener(StartZoneWeaponShooting);
        }
    }

    private IEnumerator ProjectileWeaponShooting(Data_Weapon weapon, bool isPrimaryWeapon)
    {
        while (CanShot(weapon))
        {
            //Each loop create one object
            for (int i = 0; i < weapon.ObjectsByShot; i++)
            {
                //Spawn projectile and configure it
                weapon.CurrentAmmunition--;

                GameObject _currentProjectile = Instantiate(weapon.Object, transform.position, Quaternion.identity);
                ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

                float randomAngle = Random.Range(-weapon.InaccuracyAngle, weapon.InaccuracyAngle);
                Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up)
                    * new Vector3(PlayerAimManager.Instance.AimDirection.x, 0, PlayerAimManager.Instance.AimDirection.y);
                _currentProjectileBehaviourRef.Direction = shootDirection.normalized;
                _currentProjectileBehaviourRef.Speed = weapon.TravelSpeed;
                _currentProjectileBehaviourRef.Range = weapon.Range;
                _currentProjectile.layer = _attackLayer;

                _currentProjectile.SetActive(true);

                //Little security to avoid waiting if there is only one object to spawn
                if (weapon.ObjectsByShot > 1)
                    yield return new WaitForSeconds(weapon.TimeBetweenObjectsOfOneShot);
            }
            yield return new WaitForSeconds(weapon.AttackSpeed);

            //Check if an input ask to stop the coroutine
            if (CheckCoroutineNeedToStop(isPrimaryWeapon))
                yield break;
        }
        //Stop the coroutine since the weapon cannot shoot anymore
        if (isPrimaryWeapon)
        {
            StopPrimaryCoroutine();
            yield break;
        }
        else
        {
            StopSecondaryCoroutine();
            yield break;
        }
    }

    private IEnumerator ZoneWeaponShooting(Data_Weapon weapon, bool isPrimaryWeapon)
    {
        if( CanShot(weapon) )
        {
            //Each loop create one object
            for (int i = 0; i < weapon.ObjectsByShot; i++)
            {
                //Spawn projectile and configure it
                GameObject _currentProjectileZone = Instantiate(weapon.Object, transform.position, Quaternion.identity);
                ProjectileZoneBehaviour _currentProjetileZoneBehaviourRef = _currentProjectileZone.GetComponent<ProjectileZoneBehaviour>();

                _currentProjetileZoneBehaviourRef.Speed = weapon.TravelSpeed;
                _currentProjetileZoneBehaviourRef.ZoneRadius = weapon.ZoneRadius;
                _currentProjetileZoneBehaviourRef.Target = PlayerAimManager.Instance.ZoneAimTargets[i];
                _currentProjetileZoneBehaviourRef.Trajectory = weapon.Trajectory;
                _currentProjectileZone.layer = _attackLayer;

                _currentProjectileZone.SetActive(true);

                //Little security to avoid waiting if there is only one object to spawn
                if (weapon.ObjectsByShot > 1)
                    yield return new WaitForSeconds(weapon.TimeBetweenObjectsOfOneShot);
            }
            //Auto reload
            StartCoroutine(weapon.Reload());
        }

        //Stop the coroutine, zone weapon only fire once
        if (isPrimaryWeapon)
        {
            StopPrimaryCoroutine();
            yield break;
        }
        else
        {
            StopSecondaryCoroutine();
            yield break;
        }
    }

    //Method used to start ZoneWeaponShooting coroutine
    private void StartZoneWeaponShooting(Data_Weapon weapon, bool isPrimaryWeapon)
    {
        if (isPrimaryWeapon)
        {
            _primaryShotCoroutine = StartCoroutine(ZoneWeaponShooting(weapon, true));
        }
        else
        {
            _secondaryShotCoroutine = StartCoroutine(ZoneWeaponShooting(weapon, false));
        }
    }

    //Call the correct method depending on primaryWeapon weaponType
    private void PrimaryShooting()
    {
        switch (_characterData.PrimaryWeapon.WeaponType)
        {
            case WeaponType.PROJECTILE:
                _primaryShotCoroutine = StartCoroutine(ProjectileWeaponShooting(_characterData.PrimaryWeapon, true));
                break;
            case WeaponType.ZONE:
                //Nothing, we subscribe to the event at the start
                break;
            case WeaponType.MELEE:
                break;
        }
    }

    //Call the correct method depending on secondaryWeapon weaponType
    private void SecondaryShooting()
    {
        switch (_characterData.SecondaryWeapon.WeaponType)
        {
            case WeaponType.PROJECTILE:
                _secondaryShotCoroutine = StartCoroutine(ProjectileWeaponShooting(_characterData.SecondaryWeapon, false));
                break;
            case WeaponType.ZONE:
                //Nothing, we subscribe to the event at the start
                break;
            case WeaponType.MELEE:
                break;
        }
    }

    private void StartPrimaryShot()
    {
        //Cancel secondaryShotCoroutine if currently used
        if (_secondaryShotCoroutine != null)
        {
            _stopSecondaryShotCoroutine = true; ;
        }

        //Start shooting
        _stopPrimaryShotCoroutine = false;
        PrimaryShooting();

        //Start zone aiming if needed
        if(_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StartZoneAiming(
                _characterData.PrimaryWeapon.ZoneRadius, _characterData.PrimaryWeapon.ObjectsByShot,
                _characterData.PrimaryWeapon.DistanceBetweenZones, _characterData.PrimaryWeapon.Pattern);
        }

        //Stock which weapon is currently used
        _currentWeaponUsed = _characterData.PrimaryWeapon;
    }

    private void StartSecondaryShot()
    {
        //Cancel primaryShotCoroutine if currently used
        if (_primaryShotCoroutine != null)
        {
            _stopPrimaryShotCoroutine = true;
        }

        //Start shooting
        _stopSecondaryShotCoroutine = false;
        SecondaryShooting();

        //Start zone aiming if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StartZoneAiming(
                _characterData.SecondaryWeapon.ZoneRadius, _characterData.SecondaryWeapon.ObjectsByShot,
                _characterData.SecondaryWeapon.DistanceBetweenZones, _characterData.SecondaryWeapon.Pattern);
        }

        //Stock which weapon is currently used
        _currentWeaponUsed = _characterData.SecondaryWeapon;
    }

    private void StopPrimaryShot()
    {
        //Flag to true, so when next iteration of object spawn is done the coroutine will be stopped
        _stopPrimaryShotCoroutine = true;

        //Stop zone aiming if needed
        if (_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StopZoneAiming();
        }

        //Invoke event stop shooting
        event_primaryShotInputEnded.Invoke(_characterData.PrimaryWeapon, true);

        //If other input is pressed we directly use it
        if (_controlsMap.Gameplay.SecondaryShot.IsPressed())
        {
            StartSecondaryShot();
        }
    }

    private void StopSecondaryShot()
    {
        //Flag to true, so when next iteration of object spawn is done the coroutine will be stopped
        _stopSecondaryShotCoroutine = true;

        //Stop zone aiming if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StopZoneAiming();
        }

        //Invoke event stop shooting
        event_secondaryShotInputEnded.Invoke(_characterData.SecondaryWeapon, false);

        //If other input is pressed we directly use it
        if (_controlsMap.Gameplay.PrimaryShot.IsPressed())
        {
            StartPrimaryShot();
        }
    }

    private bool CanShot(Data_Weapon weapon)
    {
        //Check if weapon isn't reloading
        if (weapon.IsReloading)
        {
            return false;
        }

        //Only projectile weapon has ammunition, so we can return true if it's not an projectile weapon
        if(weapon.WeaponType != WeaponType.PROJECTILE)
        {
            return true;
        }

        //Check remaining ammunition
        if (weapon.CurrentAmmunition - weapon.ObjectsByShot < 0)
        {
            return false;
        }
        return true;
    }

    //Check flag stop coroutine
    private bool CheckCoroutineNeedToStop(bool primaryWeapon)
    {
        if (primaryWeapon)
        {
            if (_stopPrimaryShotCoroutine)
            {
                StopPrimaryCoroutine();
                return true;
            }
        }
        else
        {
            if (_stopSecondaryShotCoroutine)
            {
                StopSecondaryCoroutine();
                return true;
            }
        }
        return false;
    }

    private void StopPrimaryCoroutine()
    {
        _stopPrimaryShotCoroutine = false;
        _primaryShotCoroutine = null;
    }

    private void StopSecondaryCoroutine()
    {
        _stopSecondaryShotCoroutine = false;
        _secondaryShotCoroutine = null;
    }

    //Reload, in priority last used weapon, if it can't be reloaded try other the other
    private void Reload()
    {
        if(_currentWeaponUsed.WeaponType  == WeaponType.PROJECTILE)
        {
            StartCoroutine(_currentWeaponUsed.Reload());
        }
        else if (_characterData.PrimaryWeapon.WeaponType == WeaponType.PROJECTILE)
        {
            StartCoroutine(_characterData.PrimaryWeapon.Reload());
        }
        else
        {
            StartCoroutine(_characterData.SecondaryWeapon.Reload());
        }
    }
}

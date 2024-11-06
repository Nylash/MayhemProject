using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using static Utilities;

public class PlayerShotManager : Singleton<PlayerShotManager>
{
    #region COMPONENTS
    private ControlsMap _controlsMap;
    #endregion

    #region VARIABLES
    private bool _primaryShot;
    private bool _secondaryShot;
    private Data_Weapon _currentWeaponUsed;
    private Coroutine _recoilCoroutine;
    #region ACCESSORS
    public Data_Character CharacterData { get => _characterData; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField, Layer] private int _attackLayer;
    #endregion

    #region EVENTS
    private UnityEvent event_primaryShotInputEnded;
    private UnityEvent event_secondaryShotInputEnded;  
    #endregion

    private void OnEnable()
    {
        _controlsMap.Gameplay.Enable();
        if (event_primaryShotInputEnded == null)
            event_primaryShotInputEnded = new UnityEvent();
        if (event_secondaryShotInputEnded == null)
            event_secondaryShotInputEnded = new UnityEvent();
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

        //Subscribe to the event if the weapon is of type ZONE, so when we release the input (we stop aiming) the shoot function is called
        if(_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            event_primaryShotInputEnded.AddListener(() 
                => StartCoroutine(ZoneWeaponShooting(_characterData.PrimaryWeapon)));
        }
        if(_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            event_secondaryShotInputEnded.AddListener(() 
                => StartCoroutine(ZoneWeaponShooting(_characterData.SecondaryWeapon)));
        }
    }

    private void Update()
    {
        if (_primaryShot)
        {
            if (CanShot(_characterData.PrimaryWeapon))
            {
                //Directly call ProjectileShooting since the boolean (_primaryShot) stay at false when this is a zone weapon
                StartCoroutine(ProjectileWeaponShooting(_characterData.PrimaryWeapon));
            }
        }
        else if (_secondaryShot)
        {
            if (CanShot(_characterData.SecondaryWeapon))
            {
                //Directly call ProjectileShooting since the boolean (_secondaryShot) stay at false when this is a zone weapon
                StartCoroutine(ProjectileWeaponShooting(_characterData.SecondaryWeapon));
            }
        }
    }

    private IEnumerator ProjectileWeaponShooting(Data_Weapon weapon)
    {
        Vector3 aimDirection = new Vector3(PlayerAimManager.Instance.AimDirection.x, 0, PlayerAimManager.Instance.AimDirection.y);

        //Each loop create one object
        for (int i = 0; i < weapon.ObjectsByShot; i++)
        {
            //Spawn projectile and configure it
            weapon.CurrentAmmunition--;

            GameObject _currentProjectile = Instantiate(weapon.Object, transform.position, Quaternion.identity);
            ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

            float randomAngle = Random.Range(-weapon.InaccuracyAngle, weapon.InaccuracyAngle);
            Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * aimDirection;
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
        //Recoil
        StartRecoil(-aimDirection, weapon.Recoil);

        //Fire rate
        weapon.IsBetweenShots = true;
        StartCoroutine(FireRateCoroutine(weapon));
    }

    private IEnumerator ZoneWeaponShooting(Data_Weapon weapon)
    {
        if(CanShot(weapon))
        {
            weapon.CurrentAmmunition--;
            //Each loop create one object
            for (int i = 0; i < weapon.ObjectsByShot; i++)
            {
                //Spawn projectile and configure it
                GameObject _currentProjectileZone = Instantiate(weapon.Object, transform.position, Quaternion.identity);
                ProjectileZoneBehaviour _currentProjetileZoneBehaviourRef = _currentProjectileZone.GetComponent<ProjectileZoneBehaviour>();

                _currentProjetileZoneBehaviourRef.Speed = weapon.TravelSpeed;
                _currentProjetileZoneBehaviourRef.Target = PlayerAimManager.Instance.ZoneAimTargets[i];
                _currentProjetileZoneBehaviourRef.Trajectory = weapon.Trajectory;
                _currentProjetileZoneBehaviourRef.AssociatedWeapon = weapon;
                _currentProjectileZone.layer = _attackLayer;

                _currentProjectileZone.SetActive(true);

                //Little security to avoid waiting if there is only one object to spawn
                if (weapon.ObjectsByShot > 1)
                    yield return new WaitForSeconds(weapon.TimeBetweenObjectsOfOneShot);
            }
            //Auto reload
            StartCoroutine(weapon.Reload());

            //Fire rate
            weapon.IsBetweenShots = false;
            StartCoroutine(FireRateCoroutine(weapon));
       }
    }

    private void StartPrimaryShot()
    {
        //Cancel secondaryShot if currently used
        if (_secondaryShot)
        {
            _secondaryShot = false;
        }

        if (PlayerAimManager.Instance.IsZoneAiming)
        {
            //Stop zone aiming with secondary weapon if needed
            if (_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
            {
                PlayerAimManager.Instance.StopZoneAiming();
            }
        }

        //Start zone aiming with primary weapon if needed
        if(_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StartZoneAiming(
                _characterData.PrimaryWeapon.ZoneRadius, _characterData.PrimaryWeapon.ObjectsByShot,
                _characterData.PrimaryWeapon.DistanceBetweenZones, _characterData.PrimaryWeapon.Pattern,
                _characterData.PrimaryWeapon.Range);
        }
        else
        {
            //Pass bool at true for Update if this is not a zone weapon
            _primaryShot = true;
        }

        //Stock which weapon is currently used
        _currentWeaponUsed = _characterData.PrimaryWeapon;
    }

    private void StartSecondaryShot()
    {
        //Cancel primaryShot if currently used
        if (_primaryShot)
        {
            _primaryShot = false;
        }

        if (PlayerAimManager.Instance.IsZoneAiming)
        {
            //Stop zone aiming with primary weapon if needed
            if (_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
            {
                PlayerAimManager.Instance.StopZoneAiming();
            }
        }

        //Start zone aiming with secondary weapon if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StartZoneAiming(
                _characterData.SecondaryWeapon.ZoneRadius, _characterData.SecondaryWeapon.ObjectsByShot,
                _characterData.SecondaryWeapon.DistanceBetweenZones, _characterData.SecondaryWeapon.Pattern,
                _characterData.SecondaryWeapon.Range);
        }
        else
        {
            //Pass bool at true for Update if this is not a zone weapon
            _secondaryShot = true;
        }

        //Stock which weapon is currently used
        _currentWeaponUsed = _characterData.SecondaryWeapon;
    }

    private void StopPrimaryShot()
    {
        _primaryShot = false;

        //Stop zone aiming if needed
        if (_characterData.PrimaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StopZoneAiming();
        }

        //Invoke event stop shooting, so if weapon is a zone weapon it can shoot
        event_primaryShotInputEnded.Invoke();

        //If other input is pressed we directly use it
        if (_controlsMap.Gameplay.SecondaryShot.IsPressed())
        {
            StartSecondaryShot();
        }
    }

    private void StopSecondaryShot()
    {
        _secondaryShot = false;

        //Stop zone aiming if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.ZONE)
        {
            PlayerAimManager.Instance.StopZoneAiming();
        }

        //Invoke event stop shooting, so if weapon is a zone weapon it can shoot
        event_secondaryShotInputEnded.Invoke();

        //If other input is pressed we directly use it
        if (_controlsMap.Gameplay.PrimaryShot.IsPressed())
        {
            StartPrimaryShot();
        }
    }

    private bool CanShot(Data_Weapon weapon)
    {
        //Check if weapon isn't between shots
        if (weapon.IsBetweenShots)
        {
            return false;
        }

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

    private IEnumerator FireRateCoroutine(Data_Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.FireRate);

        weapon.IsBetweenShots = false;
    }

    private void Reload()
    {
        if (_currentWeaponUsed.CanBeReloaded())
        {
            StartCoroutine(_currentWeaponUsed.Reload());
        }
        else if (_characterData.PrimaryWeapon.CanBeReloaded())
        {
            StartCoroutine(_characterData.PrimaryWeapon.Reload());
        }
        else if (_characterData.SecondaryWeapon.CanBeReloaded())
        {
            StartCoroutine(_characterData.SecondaryWeapon.Reload());
        }
    }

    private void StartRecoil(Vector3 direction, float recoil)
    {
        //Cancel previous recoil coroutine if there is one
        if (_recoilCoroutine != null)
        {
            StopCoroutine(_recoilCoroutine);
        }

        _recoilCoroutine = StartCoroutine(Recoil(direction, recoil));
    }

    private IEnumerator Recoil(Vector3 direction, float recoil)
    {
        //Apply recoil, and progressively reduce it
        PlayerMovementManager.Instance.RecoilDirection = direction;
        PlayerMovementManager.Instance.RecoilStrength = recoil;
        yield return new WaitForSeconds(.1f);
        PlayerMovementManager.Instance.RecoilStrength /= 2;
        yield return new WaitForSeconds(.2f);
        PlayerMovementManager.Instance.RecoilStrength = 0f;

        _recoilCoroutine = null;
    }
}

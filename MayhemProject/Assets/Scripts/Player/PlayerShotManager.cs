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

        //Subscribe to the event if the weapon is of type THROWABLE, so when we release the input (we stop aiming) the shoot function is called
        if (_characterData.PrimaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            event_primaryShotInputEnded.AddListener(() 
                => StartCoroutine(ThrowableWeaponShooting(_characterData.PrimaryWeapon)));
        }
        if(_characterData.SecondaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            event_secondaryShotInputEnded.AddListener(() 
                => StartCoroutine(ThrowableWeaponShooting(_characterData.SecondaryWeapon)));
        }
    }

    private void Update()
    {
        if (_primaryShot)
        {
            if (CanShot(_characterData.PrimaryWeapon))
            {
                //Directly call ProjectileShooting since the boolean (_primaryShot) stay at false when this is a throwable weapon
                StartCoroutine(ProjectileWeaponShooting(_characterData.PrimaryWeapon));
            }
        }
        else if (_secondaryShot)
        {
            if (CanShot(_characterData.SecondaryWeapon))
            {
                //Directly call ProjectileShooting since the boolean (_secondaryShot) stay at false when this is a throwable weapon
                StartCoroutine(ProjectileWeaponShooting(_characterData.SecondaryWeapon));
            }
        }
    }

    private IEnumerator ProjectileWeaponShooting(Data_Weapon weapon)
    {
        Vector3 aimDirection = new Vector3(PlayerAimManager.Instance.AimDirection.x, 0, PlayerAimManager.Instance.AimDirection.y);

        //Each loop create one object
        for (int i = 0; i < weapon.ObjectsByBurst; i++)
        {
            //Spawn projectile and configure it
            weapon.CurrentAmmunition--;

            GameObject _currentProjectile = Instantiate(weapon.Object, transform.position, Quaternion.identity);
            ProjectileBehaviour _currentProjectileBehaviourRef = _currentProjectile.GetComponent<ProjectileBehaviour>();

            float randomAngle = Random.Range(-weapon.InaccuracyAngle, weapon.InaccuracyAngle);
            Vector3 shootDirection = Quaternion.AngleAxis(randomAngle, Vector3.up) * aimDirection;
            _currentProjectileBehaviourRef.Direction = shootDirection.normalized;
            _currentProjectileBehaviourRef.AssociatedWeapon = weapon;
            _currentProjectile.layer = _attackLayer;

            _currentProjectile.SetActive(true);

            //Little security to avoid waiting if there is only one object to spawn
            if (weapon.ObjectsByBurst > 1)
                yield return new WaitForSeconds(weapon.BurstInternalIntervall);
        }
        //Recoil
        StartRecoil(-aimDirection, weapon.Recoil);

        //Fire rate
        weapon.IsBetweenShots = true;
        StartCoroutine(FireRateCoroutine(weapon));
    }

    private IEnumerator ThrowableWeaponShooting(Data_Weapon weapon)
    {
        if(CanShot(weapon))
        {
            weapon.CurrentAmmunition--;
            //Each loop create one object
            for (int i = 0; i < weapon.ObjectsByBurst; i++)
            {
                //Spawn projectile and configure it
                GameObject _currentProjectileZone = Instantiate(weapon.Object, transform.position, Quaternion.identity);
                ThrowableProjectile _currentProjetileZoneBehaviourRef = _currentProjectileZone.GetComponent<ThrowableProjectile>();

                _currentProjetileZoneBehaviourRef.Target = PlayerAimManager.Instance.ThrowableTargets[i];
                _currentProjetileZoneBehaviourRef.AssociatedWeapon = weapon;
                _currentProjectileZone.layer = _attackLayer;

                _currentProjectileZone.SetActive(true);

                //Little security to avoid waiting if there is only one object to spawn
                if (weapon.ObjectsByBurst > 1)
                    yield return new WaitForSeconds(weapon.BurstInternalIntervall);
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

        if (PlayerAimManager.Instance.IsThrowableAiming)
        {
            //Stop throwable aiming with secondary weapon if needed
            if (_characterData.SecondaryWeapon.WeaponType == WeaponType.THROWABLE)
            {
                PlayerAimManager.Instance.StopThrowableAiming();
            }
        }

        //Start throwable aiming with primary weapon if needed
        if (_characterData.PrimaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            PlayerAimManager.Instance.StartThrowableAiming(
                _characterData.PrimaryWeapon.ThrowableRadius, _characterData.PrimaryWeapon.ObjectsByBurst,
                _characterData.PrimaryWeapon.DistanceBetweenThrowables, _characterData.PrimaryWeapon.Pattern,
                _characterData.PrimaryWeapon.Range);
        }
        else
        {
            //Pass bool at true for Update if this is not a throwable weapon
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

        if (PlayerAimManager.Instance.IsThrowableAiming)
        {
            //Stop throwable aiming with primary weapon if needed
            if (_characterData.PrimaryWeapon.WeaponType == WeaponType.THROWABLE)
            {
                PlayerAimManager.Instance.StopThrowableAiming();
            }
        }

        //Start throwable aiming with secondary weapon if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            PlayerAimManager.Instance.StartThrowableAiming(
                _characterData.SecondaryWeapon.ThrowableRadius, _characterData.SecondaryWeapon.ObjectsByBurst,
                _characterData.SecondaryWeapon.DistanceBetweenThrowables, _characterData.SecondaryWeapon.Pattern,
                _characterData.SecondaryWeapon.Range);
        }
        else
        {
            //Pass bool at true for Update if this is not a throwable weapon
            _secondaryShot = true;
        }

        //Stock which weapon is currently used
        _currentWeaponUsed = _characterData.SecondaryWeapon;
    }

    private void StopPrimaryShot()
    {
        _primaryShot = false;

        //Stop throwable aiming if needed
        if (_characterData.PrimaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            PlayerAimManager.Instance.StopThrowableAiming();
        }

        //Invoke event stop shooting, so if weapon is a throwable weapon it can shoot
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

        //Stop throwable aiming if needed
        if (_characterData.SecondaryWeapon.WeaponType == WeaponType.THROWABLE)
        {
            PlayerAimManager.Instance.StopThrowableAiming();
        }

        //Invoke event stop shooting, so if weapon is a throwable weapon it can shoot
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
        if (weapon.CurrentAmmunition - weapon.ObjectsByBurst < 0)
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

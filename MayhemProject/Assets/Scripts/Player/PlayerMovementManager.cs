using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using static Utilities;
using UnityEngine.UI;

public class PlayerMovementManager : Singleton<PlayerMovementManager>
{
    #region COMPONENTS
    private ControlsMap _controlsMap;
    private CharacterController _controller;
    #endregion

    #region VARIABLES
    private PlayerBehaviorState _currentBehavior = PlayerBehaviorState.IDLE;
    private Vector2 _movementDirection;
    #region ACCESSORS
    public Vector2 MovementDirection { get => _movementDirection; }
    public PlayerBehaviorState CurrentBehavior { get => _currentBehavior; set => _currentBehavior = value; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField] private Image _imageDodgeReloading;
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent event_inputMovementIsStopped;
    #endregion

    private void OnEnable()
    {
        _controlsMap.Gameplay.Enable();
        if (event_inputMovementIsStopped == null)
            event_inputMovementIsStopped = new UnityEvent();
    }
    private void OnDisable() => _controlsMap.Gameplay.Disable();

    protected override void OnAwake()
    {
        _controlsMap = new ControlsMap();

        _controlsMap.Gameplay.Movement.performed += ctx => ReadMovementDirection(ctx.ReadValue<Vector2>().normalized);
        _controlsMap.Gameplay.Movement.canceled += ctx => StopReadMovementDirection();
        _controlsMap.Gameplay.Dodge.performed += ctx => StartCoroutine(Dodging());

        _controller = GetComponent<CharacterController>();
        _characterData.event_dodgeAvailabilityUpdated.AddListener(DodgeAvailabilyUpdated);
    }

    private void Update()
    {
        //Basic movement using Unity CharacterController, and apply gravity on Y to avoid floating
        //Then set Angle float in running blend tree (angle between _movementDirection and AimDirection)
        if (_currentBehavior == PlayerBehaviorState.MOVE)
        {
            _controller.Move(new Vector3(_movementDirection.x, -_characterData.GravityForce, _movementDirection.y) * _characterData.MovementSpeed * Time.deltaTime);
        }
        //Always applying gravity, also when not moving, only exception is while dodging
        else if (_currentBehavior != PlayerBehaviorState.DODGE && !_controller.isGrounded)
        {
            _controller.Move(new Vector3(0, -_characterData.GravityForce, 0) * _characterData.MovementSpeed * Time.deltaTime);
        }
    }

    private IEnumerator Dodging()
    {
        if (_characterData.DodgeIsReady)
        {
            StartDodging();

            float startTime = Time.time;

            //Store direction ate the begining of the dodge, so player dodge in straight line
            Vector2 dashDirection = _movementDirection;

            //Replace Update movement logic by this during DashDuration
            while (Time.time < startTime + _characterData.DodgeDuration)
            {
                _controller.Move(new Vector3(dashDirection.x, 0, dashDirection.y) * _characterData.DodgeSpeed * Time.deltaTime);
                yield return null;
            }

            StopDodging();

            yield return new WaitForSeconds(_characterData.DodgeCD - _characterData.DodgeDuration);
            _characterData.DodgeIsReady = true;
        }
    }

    private void StartDodging()
    {
        _currentBehavior = PlayerBehaviorState.DODGE;
        _characterData.DodgeIsReady = false;
    }

    private void StopDodging()
    {
        _currentBehavior = PlayerBehaviorState.IDLE;
        ////If movement input is pressed we directly start using it
        if (_controlsMap.Gameplay.Movement.IsPressed())
        {
            ReadMovementDirection(_movementDirection);
        }
    }

    private void ReadMovementDirection(Vector2 direction)
    {
        //Avoid dead zone reading (performed is called also in dead zone :/)
        if (direction != Vector2.zero)
        {
            //Update _movementDirection permanently so we can potentially aim with it
            _movementDirection = direction;
            if (_currentBehavior != PlayerBehaviorState.DODGE)
            {
                _currentBehavior = PlayerBehaviorState.MOVE;
            }
        }
    }

    private void StopReadMovementDirection()
    {
        //Go back to Idle if we cancel input while only moving
        if(_currentBehavior == PlayerBehaviorState.MOVE)
        {
            _currentBehavior = PlayerBehaviorState.IDLE;
        }
        //Notify this to update new LastStickDirection
        event_inputMovementIsStopped.Invoke();
    }

    private void DodgeAvailabilyUpdated(bool dodgeIsReady)
    {
        if (dodgeIsReady)
        {
            _imageDodgeReloading.fillAmount = 0;
        }
        else
        {
            StartCoroutine(DodgeReloadingUI());
        }
    }

    private IEnumerator DodgeReloadingUI()
    {
        float timer = 0f;

        while (timer < _characterData.DodgeCD)
        {
            timer += Time.deltaTime;
            _imageDodgeReloading.fillAmount = timer / _characterData.DodgeCD;
            yield return new WaitForEndOfFrame();
        }
        yield break;
    }
}

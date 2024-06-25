using UnityEngine;
using static Utilities;
using UnityEngine.InputSystem;

public class PlayerAimManager : Singleton<PlayerAimManager>
{
    #region COMPONENTS
    [Header("COMPONENTS")]
    [SerializeField] private Animator _orientationAnimator;
    [SerializeField] private Transform _zoneAimGuideTransform;
    private ControlsMap _controlsMap;
    private PlayerInput _playerInput;
    #endregion

    #region VARIABLES
    private Vector2 _stickDirection;
    private Vector2 _mouseDirection;
    private Vector2 _aimDirection;
    private Vector2 _lastStickDirection;
    private bool _isZoneAiming;
    private Vector3 _zoneAimGuideInitialPos;
    private Vector3 _zoneAimTarget;
    #region ACCESSORS
    public Vector2 AimDirection { get => _aimDirection; }
    public Vector3 ZoneAimTarget { get => _zoneAimTarget; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField] private float _zoneAimGuideSpeed = 0.3f;
    #endregion

    private void OnEnable()
    {
        _controlsMap.Gameplay.Enable();
        PlayerMovementManager.Instance.event_inputMovementIsStopped.AddListener(UpdateLastStickDirection);
    }
    private void OnDisable()
    {
        _controlsMap.Gameplay.Disable();
        if (!this.gameObject.scene.isLoaded) return;//Avoid null ref
        PlayerMovementManager.Instance.event_inputMovementIsStopped.RemoveListener(UpdateLastStickDirection);
    }

    protected override void OnAwake()
    {
        _controlsMap = new ControlsMap();

        _controlsMap.Gameplay.AimStick.performed += ctx => _stickDirection = ctx.ReadValue<Vector2>();
        _controlsMap.Gameplay.AimStick.canceled += ctx => StopReadingStickDirection();
        _controlsMap.Gameplay.AimMouse.performed += ctx => _mouseDirection = ctx.ReadValue<Vector2>();

        _playerInput = GetComponent<PlayerInput>();

        _zoneAimGuideInitialPos = _zoneAimGuideTransform.position;
    }

    private void Update()
    {
        if (PlayerMovementManager.Instance.CurrentBehavior != PlayerBehaviorState.DODGE)
        {
            CalculateAimDirection();
            if (!_isZoneAiming)
            {
                Aiming();
            }
            else
            {
                ZoneAiming();
            }
        }
        //Player is dodging, we directly set the float to MovementDirection without smoothing it
        else
        {
            _orientationAnimator.SetFloat("InputX", PlayerMovementManager.Instance.MovementDirection.x);
            _orientationAnimator.SetFloat("InputY", PlayerMovementManager.Instance.MovementDirection.y);
        }
    }

    private void Aiming()
    {
        //Mouse or right stick is used
        if (_aimDirection != Vector2.zero)
        {
            _orientationAnimator.SetFloat("InputX", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputX"), _aimDirection.x, _characterData.RotationSpeed));
            _orientationAnimator.SetFloat("InputY", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputY"), _aimDirection.y, _characterData.RotationSpeed));
        }
        //Right stick not used but left is, so we aim with it
        else if (_controlsMap.Gameplay.Movement.IsPressed())
        {
            _orientationAnimator.SetFloat("InputX", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputX"), PlayerMovementManager.Instance.MovementDirection.x, _characterData.RotationSpeed));
            _orientationAnimator.SetFloat("InputY", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputY"), PlayerMovementManager.Instance.MovementDirection.y, _characterData.RotationSpeed));
        }
        //No direction is given (neither movement nor aim) so we look at the last one register
        else
        {
            _orientationAnimator.SetFloat("InputX", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputX"), _lastStickDirection.x, _characterData.RotationSpeed));
            _orientationAnimator.SetFloat("InputY", Mathf.MoveTowards(_orientationAnimator.GetFloat("InputY"), _lastStickDirection.y, _characterData.RotationSpeed));
        }
    }

    private void ZoneAiming()
    {
        //Using keyboard & mouse so we move zone aim guide to cursor position (if the raycast it nothing we aim to the player position)
        if (_playerInput.currentControlScheme == "Keyboard")
        {
            Ray ray = Camera.main.ScreenPointToRay(_mouseDirection);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            Vector3 target = transform.position;
            if (plane.Raycast(ray, out distance))
            {
                target = ray.GetPoint(distance);
            }
            _zoneAimGuideTransform.position = new Vector3(target.x, _zoneAimGuideInitialPos.y, target.z);
        }
        else
        {
            //Using gamepad we move zone aim guide along aimDirection
            _zoneAimGuideTransform.position += (new Vector3(_aimDirection.x, 0, _aimDirection.y) * _zoneAimGuideSpeed);
        }

        //Look to zone aim guide
        Vector3 dir = _zoneAimGuideTransform.position - transform.position;
        _orientationAnimator.SetFloat("InputX", dir.x);
        _orientationAnimator.SetFloat("InputY", dir.z);
    }

    private void CalculateAimDirection()
    {
        //Get direction from mouse position if player is currently using Keyboard control scheme
        if (_playerInput.currentControlScheme == "Keyboard")
        {
            Ray ray = Camera.main.ScreenPointToRay(_mouseDirection);
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            float distance;
            if (plane.Raycast(ray, out distance))
            {
                Vector3 target = ray.GetPoint(distance) - transform.position;
                _aimDirection = new Vector2(target.x, target.z).normalized;
            }
        }
        //Get direction from gamepad
        else
        {
            _aimDirection = _stickDirection;
        }
    }

    private void StopReadingStickDirection()
    {
        //Store last direction given by the stick
        _lastStickDirection = _stickDirection;
        _stickDirection = Vector2.zero;
    }

    private void UpdateLastStickDirection()
    {
        //If when player stop moving he wasn't aiming, we override _lastStickDirection with the MovementDirection, so he keeps looking forward
        if (_stickDirection == Vector2.zero)
        {
            _lastStickDirection = PlayerMovementManager.Instance.MovementDirection;
        }
    }

    public void StartZoneAiming(float zoneRadius)
    {
        //Activate zone aim guide and initializing it
        _zoneAimGuideTransform.localScale = Vector3.one * zoneRadius;
        _zoneAimGuideTransform.position = new Vector3(transform.position.x, _zoneAimGuideInitialPos.y, transform.position.z);
        _isZoneAiming = true;
        _zoneAimGuideTransform.gameObject.SetActive(true);
    }

    public void StopZoneAiming()
    {
        //Deactivate zone aim guide and register last position
        _isZoneAiming = false;
        _zoneAimTarget = _zoneAimGuideTransform.position;
        _zoneAimGuideTransform.gameObject.SetActive(false);
    }
}

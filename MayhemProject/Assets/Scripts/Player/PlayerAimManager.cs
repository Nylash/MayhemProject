using UnityEngine;
using static Utilities;
using UnityEngine.InputSystem;

public class PlayerAimManager : Singleton<PlayerAimManager>
{
    #region COMPONENTS
    [Header("COMPONENTS")]
    [SerializeField] private Animator _orientationAnimator;
    private ControlsMap _controlsMap;
    private PlayerInput _playerInput;
    #endregion

    #region VARIABLES
    private Vector2 _stickDirection;
    private Vector2 _mouseDirection;
    private Vector2 _aimDirection;
    private Vector2 _lastStickDirection;

    #region ACCESSORS
    public Vector2 AimDirection { get => _aimDirection; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
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
    }

    private void Update()
    {
        if (PlayerMovementManager.Instance.CurrentBehavior != PlayerBehaviorState.DODGE)
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
        //Player is dodging, we directly set the float to MovementDirection without smoothing it
        else
        {
            _orientationAnimator.SetFloat("InputX", PlayerMovementManager.Instance.MovementDirection.x);
            _orientationAnimator.SetFloat("InputY", PlayerMovementManager.Instance.MovementDirection.y);
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
}

using UnityEngine;
using static Utilities;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using static UnityEditor.PlayerSettings;

public class PlayerAimManager : Singleton<PlayerAimManager>
{
    #region COMPONENTS
    [Header("COMPONENTS")]
    [SerializeField] private Animator _orientationAnimator;
    [SerializeField] private Transform _throwableGuideTransform;
    private ControlsMap _controlsMap;
    private PlayerInput _playerInput;
    #endregion

    #region VARIABLES
    private Vector2 _stickDirection;
    private Vector2 _mouseDirection;
    private Vector2 _aimDirection;
    private Vector2 _lastStickDirection;
    private bool _isThrowableAiming;
    private List<Vector3> _throwableTargets = new List<Vector3>();
    private List<Transform> _additionalsThrowableGuideTransforms = new List<Transform>();
    private Vector3 _ThrowableGuideParentInitialPos;
    private float _throwableRange;
    #region ACCESSORS
    public Vector2 AimDirection
    {
        get
        {
            //Mouse or right stick is used
            if (_aimDirection != Vector2.zero)
            {
                return _aimDirection;
            }
            //Right stick not used but left is, so we aim with it
            else if (_controlsMap.Gameplay.Movement.IsPressed())
            {
                return PlayerMovementManager.Instance.MovementDirection;
            }
            //No direction is given (neither movement nor aim) so we look at the last one register
            else if(_lastStickDirection != Vector2.zero)
            {
                return _lastStickDirection;
            }
            else
            {
                return Vector3.one;
            }
        }
    }
    public List<Vector3> ThrowableTargets { get => _throwableTargets; }
    public bool IsThrowableAiming { get => _isThrowableAiming; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField] private GameObject _throwableGuideObject;
    [SerializeField] private float _throwableGuideSpeed = 0.1f;
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

        _ThrowableGuideParentInitialPos = _throwableGuideTransform.parent.localPosition;
    }

    private void Update()
    {
        if (PlayerMovementManager.Instance.CurrentBehavior != PlayerBehaviorState.DODGE)
        {
            CalculateAimDirection();
            if (!_isThrowableAiming)
            {
                Aiming();
            }
            else
            {
                ThrowableAiming();
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

    private void ThrowableAiming()
    {
        //Using keyboard & mouse so we move throwable guide to cursor position (if the raycast it nothing we aim to the player position)
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

            // Get the new position for the object
            Vector3 movement = new Vector3(target.x, _throwableGuideTransform.parent.position.y, target.z);

            // Calculate the distance of the new position from the center point. Keep the direction
            // the same but clamp the length to the specified radius
            Vector3 offset = movement - transform.position;
            _throwableGuideTransform.parent.position = transform.position + Vector3.ClampMagnitude(offset, _throwableRange);
        }
        else
        {
            //Using gamepad we move throwable guide along aimDirection
            _throwableGuideTransform.parent.position += (new Vector3(_aimDirection.x, 0, _aimDirection.y) * _throwableGuideSpeed);

            // Get the new position for the object
            Vector3 movement = _throwableGuideTransform.parent.position + (new Vector3(_aimDirection.x, 0, _aimDirection.y) * _throwableGuideSpeed);

            // Calculate the distance of the new position from the center point. Keep the direction
            // the same but clamp the length to the specified radius
            Vector3 offset = movement - transform.position;
            _throwableGuideTransform.parent.position = transform.position + Vector3.ClampMagnitude(offset, _throwableRange);
        }

        //Look to throwable guide
        Vector3 dir = _throwableGuideTransform.parent.position - transform.position;
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

    public void StartThrowableAiming(float throwableRadius, int numberOfThrowables, float offsetAdditionalsThrowables, ThrowablePattern pattern, float range)
    {
        _throwableRange = range;

        //Activate throwable guide and initializing it
        _throwableGuideTransform.localScale = Vector3.one * throwableRadius;
        _throwableGuideTransform.parent.localPosition = _ThrowableGuideParentInitialPos;
        _throwableGuideTransform.parent.rotation = Quaternion.identity;
        _throwableGuideTransform.localPosition = Vector3.zero;

        //Cloning additionals throwable guide if we spawn more than one object
        for (int i = 1; i < numberOfThrowables; i++)
        {
            _additionalsThrowableGuideTransforms.Add(Instantiate(_throwableGuideObject, _throwableGuideTransform.position, _throwableGuideTransform.rotation).transform);
            _additionalsThrowableGuideTransforms[i - 1].parent = _throwableGuideTransform.parent;
            _additionalsThrowableGuideTransforms[i - 1].localScale = Vector3.one * throwableRadius;
        }
        //Place the guide according to the pattern
        ApplyOffsetPattern(offsetAdditionalsThrowables, pattern);

        //Activate everything
        _isThrowableAiming = true;
        _throwableGuideTransform.gameObject.SetActive(true);
        foreach (Transform item in _additionalsThrowableGuideTransforms)
        {
            item.gameObject.SetActive(true);
        }

        //Clean previous targets
        _throwableTargets.Clear();
    }

    public void StopThrowableAiming()
    {
        //Deactivate throwable guide and register last position
        _isThrowableAiming = false;
        _throwableTargets.Add(_throwableGuideTransform.position);
        _throwableGuideTransform.gameObject.SetActive(false);

        //Do the same for additionals guides
        foreach (Transform item in _additionalsThrowableGuideTransforms)
        {
            _throwableTargets.Add(item.position);
            Destroy(item.gameObject);
        }
        _additionalsThrowableGuideTransforms.Clear();

        //Reset all positions
        _throwableGuideTransform.parent.localPosition = _ThrowableGuideParentInitialPos;
        _throwableGuideTransform.parent.rotation = Quaternion.identity;
        _throwableGuideTransform.localPosition = Vector3.zero;
    }

    //Calculate additionals throwable guides positions
    private void ApplyOffsetPattern(float offset, ThrowablePattern pattern)
    {
        bool isOdd = IsIntOdd(_additionalsThrowableGuideTransforms.Count + 1);

        //Apply pattern depending on total number of guides
        if (isOdd)
        {
            //Only move additionals guides
            switch (pattern) 
            {
                case ThrowablePattern.LINE:
                    for (int i = 0; i < _additionalsThrowableGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        _additionalsThrowableGuideTransforms[i].position += new Vector3(pos, 0, 0);
                    }
                    break;
                case ThrowablePattern.ARC:
                    for (int i = 0; i < _additionalsThrowableGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        _additionalsThrowableGuideTransforms[i].position += new Vector3(pos, 0, Mathf.Abs(pos)/2);
                    }
                    break;
            }
        }
        else
        {
            //Move basic guide and additionals
            switch (pattern)
            {
                case ThrowablePattern.LINE:
                    _throwableGuideTransform.position += new Vector3(-offset / 2, 0, 0);
                    for (int i = 0; i < _additionalsThrowableGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        //Calculate new position from basic zone position
                        Vector3 newPosition = _throwableGuideTransform.position + new Vector3(pos, 0, 0);

                        //Apply new position
                        _additionalsThrowableGuideTransforms[i].position = newPosition;
                    }
                    break;
                case ThrowablePattern.ARC:
                    _throwableGuideTransform.position += new Vector3(-offset / 2, 0, 0);
                    for (int i = 0; i < _additionalsThrowableGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float posX = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;
                        //(i + 3) / 2 so we have (1, 2, 2, 3, 3...)
                        //the -1 is to alignate everything with the basic zone
                        float posZ = (i + 3) / 2 * (offset / 2) - 1;

                        //Calculate new position from basic zone position
                        Vector3 newPosition = _throwableGuideTransform.position + new Vector3(posX, 0, posZ);

                        //Apply new position
                        _additionalsThrowableGuideTransforms[i].position = newPosition;
                    }
                    break;
            }
        }
    }
}

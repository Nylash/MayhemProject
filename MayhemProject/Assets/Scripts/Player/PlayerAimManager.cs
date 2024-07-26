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
    private List<Vector3> _zoneAimTargets = new List<Vector3>();
    private List<Transform> _additionalsZoneAimGuideTransforms = new List<Transform>();
    private Vector3 _zoneAimParentInitialPos;
    private float _zoneAimingRange;
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
    public List<Vector3> ZoneAimTargets { get => _zoneAimTargets; }
    public bool IsZoneAiming { get => _isZoneAiming; }
    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    [SerializeField] private GameObject _zoneAimGuideObject;
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

        _zoneAimParentInitialPos = _zoneAimGuideTransform.parent.localPosition;
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

            // Get the new position for the object
            Vector3 movement = new Vector3(target.x, _zoneAimGuideTransform.parent.position.y, target.z);

            // Calculate the distance of the new position from the center point. Keep the direction
            // the same but clamp the length to the specified radius
            Vector3 offset = movement - transform.position;
            _zoneAimGuideTransform.parent.position = transform.position + Vector3.ClampMagnitude(offset, _zoneAimingRange);
        }
        else
        {
            //Using gamepad we move zone aim guide along aimDirection
            _zoneAimGuideTransform.parent.position += (new Vector3(_aimDirection.x, 0, _aimDirection.y) * _zoneAimGuideSpeed);

            // Get the new position for the object
            Vector3 movement = _zoneAimGuideTransform.parent.position + (new Vector3(_aimDirection.x, 0, _aimDirection.y) * _zoneAimGuideSpeed);

            // Calculate the distance of the new position from the center point. Keep the direction
            // the same but clamp the length to the specified radius
            Vector3 offset = movement - transform.position;
            _zoneAimGuideTransform.parent.position = transform.position + Vector3.ClampMagnitude(offset, _zoneAimingRange);
        }

        //Look to zone aim guide
        Vector3 dir = _zoneAimGuideTransform.parent.position - transform.position;
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

    public void StartZoneAiming(float zoneRadius, int numberOfZones, float offsetAdditionalsZones, ZonePattern pattern, float range)
    {
        _zoneAimingRange = range;

        //Activate zone aim guide and initializing it
        _zoneAimGuideTransform.localScale = Vector3.one * zoneRadius;
        _zoneAimGuideTransform.parent.localPosition = _zoneAimParentInitialPos;
        _zoneAimGuideTransform.parent.rotation = Quaternion.identity;
        _zoneAimGuideTransform.localPosition = Vector3.zero;

        //Cloning additionals zones if we spawn more than one object
        for (int i = 1; i < numberOfZones; i++)
        {
            _additionalsZoneAimGuideTransforms.Add(Instantiate(_zoneAimGuideObject, _zoneAimGuideTransform.position, _zoneAimGuideTransform.rotation).transform);
            _additionalsZoneAimGuideTransforms[i - 1].parent = _zoneAimGuideTransform.parent;
            _additionalsZoneAimGuideTransforms[i - 1].localScale = Vector3.one * zoneRadius;
        }
        //Place the zone according to the pattern
        ApplyOffsetPattern(offsetAdditionalsZones, pattern);

        //Activate everything
        _isZoneAiming = true;
        _zoneAimGuideTransform.gameObject.SetActive(true);
        foreach (Transform item in _additionalsZoneAimGuideTransforms)
        {
            item.gameObject.SetActive(true);
        }

        //Clean previous targets
        _zoneAimTargets.Clear();
    }

    public void StopZoneAiming()
    {
        //Deactivate zone aim guide and register last position
        _isZoneAiming = false;
        _zoneAimTargets.Add(_zoneAimGuideTransform.position);
        _zoneAimGuideTransform.gameObject.SetActive(false);

        //Do the same for additionals zones
        foreach (Transform item in _additionalsZoneAimGuideTransforms)
        {
            _zoneAimTargets.Add(item.position);
            Destroy(item.gameObject);
        }
        _additionalsZoneAimGuideTransforms.Clear();

        //Reset all positions
        _zoneAimGuideTransform.parent.localPosition = _zoneAimParentInitialPos;
        _zoneAimGuideTransform.parent.rotation = Quaternion.identity;
        _zoneAimGuideTransform.localPosition = Vector3.zero;
    }

    private void ApplyOffsetPattern(float offset, ZonePattern pattern)
    {
        bool isOdd = IsIntOdd(_additionalsZoneAimGuideTransforms.Count + 1);

        //Apply pattern depending on total number of zones
        if (isOdd)
        {
            //Only move additionals zones
            switch (pattern) 
            {
                case ZonePattern.LINE:
                    for (int i = 0; i < _additionalsZoneAimGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        _additionalsZoneAimGuideTransforms[i].position += new Vector3(pos, 0, 0);
                    }
                    break;
                case ZonePattern.ARC:
                    for (int i = 0; i < _additionalsZoneAimGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        _additionalsZoneAimGuideTransforms[i].position += new Vector3(pos, 0, Mathf.Abs(pos)/2);
                    }
                    break;
            }
        }
        else
        {
            //Move basic zone and additionals
            switch (pattern)
            {
                case ZonePattern.LINE:
                    _zoneAimGuideTransform.position += new Vector3(-offset / 2, 0, 0);
                    for (int i = 0; i < _additionalsZoneAimGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float pos = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;

                        //Calculate new position from basic zone position
                        Vector3 newPosition = _zoneAimGuideTransform.position + new Vector3(pos, 0, 0);

                        //Apply new position
                        _additionalsZoneAimGuideTransforms[i].position = newPosition;
                    }
                    break;
                case ZonePattern.ARC:
                    _zoneAimGuideTransform.position += new Vector3(-offset / 2, 0, 0);
                    for (int i = 0; i < _additionalsZoneAimGuideTransforms.Count; i++)
                    {
                        //(i / 2 + 1) calculate slot (1, 1, 2, 2...)
                        //((i % 2 == 0) ? 1 : -1) calculate the sign so we have (1, -1, 2, -2...)
                        //then we apply the offset to have the right position
                        float posX = (i / 2 + 1) * ((i % 2 == 0) ? 1 : -1) * offset;
                        //(i + 3) / 2 so we have (1, 2, 2, 3, 3...)
                        //the -1 is to alignate everything with the basic zone
                        float posZ = (i + 3) / 2 * (offset / 2) - 1;

                        //Calculate new position from basic zone position
                        Vector3 newPosition = _zoneAimGuideTransform.position + new Vector3(posX, 0, posZ);

                        //Apply new position
                        _additionalsZoneAimGuideTransforms[i].position = newPosition;
                    }
                    break;
            }
        }
    }
}

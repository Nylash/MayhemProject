using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This SO is use for characters data
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Character Data")]
public class Data_Character : ScriptableObject
{
    #region CHARACTER INFO
    [Header("Character")]
    [SerializeField] private string _name = "Default";
    [SerializeField] private Data_Weapon _primaryWeapon;
    [SerializeField] private Data_Weapon _secondaryWeapon;
    #endregion

    #region RESSOURCES
    [Header("Ressources")]
    [SerializeField] private float _maxHP = 100;
    #endregion

    #region MOVEMENT VARIABLES
    [Header("Movement")]
    [SerializeField] private float _movementSpeed = 5;
    [SerializeField][Range(0, 1)] private float _rotationSpeed = .075f;
    [SerializeField] private float _gravityForce = 9.81f;
    #endregion

    #region DODGE VARIABLES
    [Header("Dodge")]
    [SerializeField] private float _dodgeSpeed = 20;
    [SerializeField] private float _dodgeCD = 1;
    [SerializeField] private float _dodgeDuration = .3f;
    #endregion

    #region RUNTIME VARIABLES
    //Be sure to reset those variables at the start of the game if needed (do it on Start so event are listened)
    [Header("Runtime")]
    private bool _dodgeIsReady;
    private float _currentHP;
    #endregion

    #region ACCESSORS
    public string Name { get => _name; }
    public float MaxHP { get => _maxHP; }
    public float MovementSpeed { get => _movementSpeed; }
    public float RotationSpeed { get => _rotationSpeed; }
    public float GravityForce { get => _gravityForce; }
    public bool DodgeIsReady { get => _dodgeIsReady; 
        set
        {
            _dodgeIsReady = value;
            event_dodgeAvailabilityUpdated.Invoke(_dodgeIsReady);
        }  
    }
    public float DodgeSpeed { get => _dodgeSpeed; }
    public float DodgeCD { get => _dodgeCD; }
    public float DodgeDuration { get => _dodgeDuration; }
    public Data_Weapon PrimaryWeapon { get => _primaryWeapon; }
    public Data_Weapon SecondaryWeapon { get => _secondaryWeapon; }
    public float CurrentHP { get => _currentHP; 
        set 
        {
            _currentHP = value;
            event_currentHPUpdated.Invoke(_currentHP);
        }  
    }
    #endregion

    #region EVENTS
    [HideInInspector] public UnityEvent<bool> event_dodgeAvailabilityUpdated;
    [HideInInspector] public UnityEvent<float> event_currentHPUpdated;
    #endregion

    private void OnEnable()
    {
        if (event_dodgeAvailabilityUpdated == null)
            event_dodgeAvailabilityUpdated = new UnityEvent<bool>();
        if (event_currentHPUpdated == null)
            event_currentHPUpdated = new UnityEvent<float>();
    }
}

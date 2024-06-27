using UnityEngine;

/// <summary>
/// This SO is use for enemies data, NO RUNTIME DATA since several instances can acces it
/// </summary>
[CreateAssetMenu(menuName = "Scriptable Objects/Enemy Data")]
public class Data_Enemy : ScriptableObject
{
    #region ENEMY INFO
    [Header("Enemy")]
    [SerializeField] private string _name = "Default";
    #endregion

    #region RESSOURCES
    [Header("Ressources")]
    [SerializeField] private float _maxHP = 100;
    #endregion

    #region ATTACK VARIABLES
    [Header("Attack")]
    [SerializeField] private float _attack = 5;
    #endregion

    #region ACCESSORS
    public string Name { get => _name; set => _name = value; }
    public float MaxHP { get => _maxHP; set => _maxHP = value; }
    public float Attack { get => _attack; set => _attack = value; }
    #endregion
}

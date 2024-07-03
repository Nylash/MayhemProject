using UnityEngine;

public class PlayerHealthManager : Singleton<PlayerHealthManager>
{
    #region COMPONENTS

    #endregion

    #region VARIABLES

    #region ACCESSORS

    #endregion
    #endregion

    #region CONFIGURATION
    [Header("CONFIGURATION")]
    [SerializeField] private Data_Character _characterData;
    #endregion

    private void Start()
    {
        //Initialize every character runtime value (be sure to add listener on OnAwake)
        _characterData.InitializePlayer();
    }

    public void TakeDamage(float damage)
    {
        _characterData.CurrentHP -= damage;
        if (_characterData.CurrentHP < 0)
        {
            _characterData.CurrentHP = 0;
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        Destroy(gameObject);
    }
}

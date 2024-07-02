using System;
using UnityEngine;
using BehaviourTree;
using UnityEngine.UI;

public abstract class BasicEnemy_BT : BehaviourTree.BehaviourTree
{
    [Header("BASE")]
    #region COMPONENTS
    [SerializeField] private Image _HPBarFill;
    [SerializeField] private GameObject _HPBar;
    #endregion

    #region VARIABLES
    private float _HP;
    #region ACCESSORS
    #endregion
    #endregion

    #region CONFIGURATION
    [SerializeField] protected Data_Enemy _enemyData;
    #endregion

    protected override Node SetupTree()
    {
        throw new NotImplementedException();
    }

    protected override void Start()
    {
        //base.Start();

        _HP = _enemyData.MaxHP;
        UpdateHPBar();
    }

    protected void LateUpdate()
    {
        //Make HP bar always face the camera
        //_HPBar.transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward, Camera.main.transform.rotation * Vector3.up);
    }

    public abstract void Initialize();

    public void TakeDamage(float damage)
    {
        _HP -= damage;
        UpdateHPBar();
        if (_HP <= 0)
        {
            Die();
        }
    }

    private void UpdateHPBar()
    {
        _HPBarFill.fillAmount = _HP / _enemyData.MaxHP;
        if (_HPBarFill.fillAmount < 1)
            _HPBar.SetActive(true);
    }


    private void Die()
    {
        Destroy(gameObject);
    }
}

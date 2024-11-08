using System.Collections.Generic;
using UnityEngine;

public class ExplosionBehaviour : MonoBehaviour
{
    private Data_Weapon _associatedWeapon;
    private List<Collider> _hitColliders = new List<Collider>();

    public Data_Weapon AssociatedWeapon { set => _associatedWeapon = value; }

    private void OnTriggerEnter(Collider other)
    {
        if (!_hitColliders.Contains(other))
        {
            if (other.CompareTag("Enemy"))
            {
                other.gameObject.GetComponentInParent<BasicEnemy_BT>().TakeDamage(_associatedWeapon.Damage);
            }
            if (other.CompareTag("Player"))
            {
                PlayerHealthManager.Instance.TakeDamage(_associatedWeapon.Damage);
            }
            _hitColliders.Add(other);
        }
    }
}

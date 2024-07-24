using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    private Data_Weapon _associatedWeapon;
    private Vector3 _direction;
    private float _speed;
    private float _range;
    private Rigidbody _rb;
    private Vector3 _birthPlace;

    public Vector3 Direction { get => _direction; set => _direction = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float Range { get => _range; set => _range = value; }
    public Data_Weapon AssociatedWeapon { get => _associatedWeapon; set => _associatedWeapon = value; }

    private void Start()
    {
        _birthPlace = transform.position;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = _direction * _speed;
    }

    private void Update()
    {
        if (Vector3.Distance(_birthPlace, transform.position) > _range)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.gameObject.GetComponentInParent<BasicEnemy_BT>().TakeDamage(_associatedWeapon.Damage);
            Destroy(gameObject);
        }
        if (other.CompareTag("Player"))
        {
            PlayerHealthManager.Instance.TakeDamage(_associatedWeapon.Damage);
            Destroy(gameObject);
        }
    }
}

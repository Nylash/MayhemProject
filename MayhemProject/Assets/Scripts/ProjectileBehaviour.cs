using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    private Vector3 _direction;
    private float _speed;
    private float _range;
    private Rigidbody _rb;
    private Vector3 _birthPlace;

    public Vector3 Direction { get => _direction; set => _direction = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float Range { get => _range; set => _range = value; }

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
}

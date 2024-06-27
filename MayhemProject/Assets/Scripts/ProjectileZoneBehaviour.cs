using UnityEngine;

public class ProjectileZoneBehaviour : MonoBehaviour
{
    private Data_Weapon _associatedWeapon;
    private Vector3 _target;
    private float _speed;
    private float _zoneRadius;
    private AnimationCurve _trajectory;
    private float _remainingDistanceRatio;
    private float _distanceToTarget;

    public Vector3 Target { get => _target; set => _target = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public float ZoneRadius { get => _zoneRadius; set => _zoneRadius = value; }
    public AnimationCurve Trajectory { get => _trajectory; set => _trajectory = value; }
    public Data_Weapon AssociatedWeapon { get => _associatedWeapon; set => _associatedWeapon = value; }

    private void Start()
    {
        //Distance only calculate on plane XZ
        _distanceToTarget = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_target.x, _target.z));
    }

    private void Update()
    {
        _remainingDistanceRatio = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_target.x, _target.z)) / _distanceToTarget;

        //if NaN we aim right under the player
        if(float.IsNaN(_remainingDistanceRatio))
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.x, 0, _target.z), _speed);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.x, _trajectory.Evaluate(1 - _remainingDistanceRatio), _target.z), _speed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Destroy(gameObject);
    }
}

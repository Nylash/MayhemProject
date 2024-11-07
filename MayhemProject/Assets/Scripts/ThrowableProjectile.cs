using UnityEngine;

public class ThrowableProjectile : MonoBehaviour
{
    private Data_Weapon _associatedWeapon;
    private Vector3 _target;
    private float _speed;
    private AnimationCurve _trajectory;
    private float _remainingDistanceRatio;
    private float _distanceToTarget;
    private GameObject _explosion;
    private bool _targetReached;

    private float _animationTotalDistance;
    private Vector3 _animationTargetScale;

    public Vector3 Target { get => _target; set => _target = value; }
    public float Speed { get => _speed; set => _speed = value; }
    public AnimationCurve Trajectory { get => _trajectory; set => _trajectory = value; }
    public Data_Weapon AssociatedWeapon { get => _associatedWeapon; set => _associatedWeapon = value; }

    private void Start()
    {
        //Distance only calculate on plane XZ
        _distanceToTarget = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_target.x, _target.z));
    }

    private void Update()
    {
        //Move to target
        if (!_targetReached)
        {
            _remainingDistanceRatio = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(_target.x, _target.z)) / _distanceToTarget;

            //if NaN we aim right under the player
            if (float.IsNaN(_remainingDistanceRatio))
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.x, 0, _target.z), _speed);
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(_target.x, _trajectory.Evaluate(1 - _remainingDistanceRatio), _target.z), _speed);
            }
        }
        //Explosion animation
        else
        {
            float animationRemainingDistance = Vector3.Distance(_explosion.transform.localScale, _animationTargetScale);

            _explosion.transform.localScale = Vector3.MoveTowards(
                _explosion.transform.localScale, 
                _animationTargetScale,
                _associatedWeapon.ExplosionSpeed.Evaluate(Mathf.Clamp01(1.0f - (animationRemainingDistance / _animationTotalDistance)))
            );

            if(_explosion.transform.localScale == _animationTargetScale)
            {
                Destroy(_explosion);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Spawn animation object
        _explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
        _explosion.layer = gameObject.layer;
        _explosion.GetComponent<ExplosionBehaviour>().AssociatedWeapon = _associatedWeapon;
        //Calculate animation values
        //2.5f is the ratio to match the zoneAimGuide size 
        _animationTargetScale = Vector3.one * _associatedWeapon.ZoneRadius * 2.5f;
        _animationTotalDistance = Vector3.Distance(_explosion.transform.localScale, _animationTargetScale);

        _targetReached = true;
        //Hide projectile
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
    }
}

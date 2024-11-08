using UnityEngine;

public class FlameBehaviour : ProjectileBehaviour
{
    [SerializeField] private AnimationCurve _speedCurve;
    [SerializeField][Range(0f,1f)] private float _percentDistanceToDie = 0.95f;

    private Vector3 _startSpeed;
    private float _fullDistance;
    private Vector3 _deathPlace;
    private float _speed;

    protected override void Start()
    {
        base.Start();

        _startSpeed = _rb.velocity;
        _deathPlace = _birthPlace + Direction * AssociatedWeapon.Range;
        _fullDistance = Vector3.Distance(_birthPlace, _deathPlace);
    }

    protected override void Update()
    {
        base.Update();

        _speed = Vector3.Magnitude(_startSpeed * _speedCurve.Evaluate(Vector3.Distance(_birthPlace, transform.position) / _fullDistance));
        _rb.velocity = Direction * _speed;
        if (Vector3.Distance(_birthPlace, transform.position) / _fullDistance > _percentDistanceToDie)
            Destroy(gameObject);
    }
}

using TMPro.EditorUtilities;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{
    private Data_Weapon _associatedWeapon;
    private Vector3 _direction;
    private Rigidbody _rb;
    private Vector3 _birthPlace;

    private GameObject _explosion;
    private bool _exploded;
    private Vector3 _animationTargetScale;
    private int _remainingPenetration;

    public Vector3 Direction { get => _direction; set => _direction = value; }
    public Data_Weapon AssociatedWeapon { get => _associatedWeapon; set => _associatedWeapon = value; }

    private void Start()
    {
        _birthPlace = transform.position;
        _rb = GetComponent<Rigidbody>();
        _rb.velocity = _direction * _associatedWeapon.TravelSpeed;
        _remainingPenetration = _associatedWeapon.Penetration;
    }

    private void Update()
    {
        if (Vector3.Distance(_birthPlace, transform.position) > _associatedWeapon.Range)
        {
            //If no splash range this is a simple projectile so we destroy it, otherwise we trigger the explosion
            if (_associatedWeapon.SplashRange == 0)
            {
                Destroy(gameObject);
            }
            else if(!_explosion)
            {
                StartExplosion();
            }
        }
        if (_exploded)
        {
            _explosion.transform.localScale = Vector3.MoveTowards(
                _explosion.transform.localScale,
                _animationTargetScale,
                0.05f + _associatedWeapon.SplashRange / 100
            );
            if (_explosion.transform.localScale == _animationTargetScale)
            {
                Destroy(_explosion);
                Destroy(gameObject);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //If no splash range this is a simple projectile, otherwise this is an explosive one
        if (_associatedWeapon.SplashRange == 0f)
        {
            if (other.CompareTag("Enemy"))
            {
                other.gameObject.GetComponentInParent<BasicEnemy_BT>().TakeDamage(_associatedWeapon.Damage);
                if (Penetration())
                    return;
            }
            if (other.CompareTag("Player"))
            {
                PlayerHealthManager.Instance.TakeDamage(_associatedWeapon.Damage);
                if (Penetration())
                    return;
            }
            //If none of the two tests succeed, it then that we hit Environnement
            Destroy(gameObject);
        }
        else
        {
            StartExplosion();
        }
    }

    private void StartExplosion()
    {
        //Spawn animation object
        _explosion = Instantiate(Resources.Load("Explosion") as GameObject, transform.position, Quaternion.identity);
        _explosion.layer = gameObject.layer;
        _explosion.GetComponent<ExplosionBehaviour>().AssociatedWeapon = _associatedWeapon;
        //Calculate animation values
        _animationTargetScale = Vector3.one * _associatedWeapon.SplashRange;

        _exploded = true;
        //Hide projectile
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        _rb.velocity = Vector3.zero;
    }

    private bool Penetration()
    {
        if (_remainingPenetration > 0)
        {
            _remainingPenetration--;
            if (_remainingPenetration == 0)
            {
                Destroy(gameObject);
            }
            return true;
        }
        else
        {
            Destroy(gameObject);
        }
        return false;
    }
}

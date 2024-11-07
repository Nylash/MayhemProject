using UnityEngine;

public class ThrowableGuidesRotation : MonoBehaviour
{
    [SerializeField] private Transform _player;

    private Vector3 _lookAtPosition;

    private void Update()
    {
        _lookAtPosition = _player.position;
        _lookAtPosition = new Vector3(_lookAtPosition.x, transform.position.y, _lookAtPosition.z);
        transform.LookAt(_lookAtPosition);
    }
}

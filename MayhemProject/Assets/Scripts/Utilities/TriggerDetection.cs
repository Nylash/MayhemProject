using UnityEngine;

public class TriggerDetection : MonoBehaviour
{
    private bool _isTriggered;

    public bool IsTriggered { get => _isTriggered; }

    private void OnTriggerEnter(Collider other)
    {
        _isTriggered = true;
    }

    private void OnTriggerExit(Collider other)
    {
        _isTriggered = false;
    }
}

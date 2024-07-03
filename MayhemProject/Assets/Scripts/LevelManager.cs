using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class LevelManager : MonoBehaviour
{
    //Move those info to game manager so we only have them in one place
    [SerializeField] private float _copOffsetY;
    [SerializeField] private float _copSpawnDetectionRadius;
    [SerializeField] private float _tankOffsetY;
    [SerializeField] private float _tankSpawnDetectionRadius;
    [SerializeField] private float _chopperOffsetY;
    [SerializeField] private float _chopperSpawnDetectionRadius;
    [SerializeField] private LayerMask _checkUnitLayer;

    private List<Collider> _zones = new List<Collider>();

    private const int MaxSpawnAttempts = 100;

    private void Awake()
    {
        foreach (Collider t in GetComponentsInChildren<Collider>())
        {
            _zones.Add(t);
        }

        //Call GameManager for level initialization
        //Dictionnary coming from game manager
        var unitsToSpawn = new Dictionary<string, int>
        {
            { "Cop", 50 },
            { "Tank", 5 },
            { "Chopper", 3 }
        };

        if (HasEnoughSpace(unitsToSpawn))
        {
            foreach (var unit in unitsToSpawn)
            {
                for (int i = 0; i < unit.Value; i++)
                {
                    Vector3 spawnPosition;
                    bool positionFound = TryGetRandomPosition(unit.Key, out spawnPosition);
                    if (positionFound)
                    {
                        EnemyFactory.CreateEnemy(unit.Key, spawnPosition);
                    }
                    else
                    {
                        Debug.LogWarning($"Unable to find a valid spawn position for {unit.Key} after multiple attempts.");
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Not enough space to spawn all units.");
        }
    }

    private bool HasEnoughSpace(Dictionary<string, int> units)
    {
        // Calculate the total required area for all units
        float totalRequiredArea = 0f;
        foreach (var unit in units)
        {
            float radius = GetSpawnDetectionRadius(unit.Key);
            totalRequiredArea += unit.Value * Mathf.PI * Mathf.Pow(radius, 2);
        }

        // Calculate the available area in all zones
        float totalAvailableArea = 0f;
        foreach (Collider zone in _zones)
        {
            totalAvailableArea += zone.bounds.size.x * zone.bounds.size.z;
        }

        // Check if the available area is sufficient for all units
        return totalAvailableArea >= totalRequiredArea;
    }

    private bool TryGetRandomPosition(string unit, out Vector3 position)
    {
        position = Vector3.zero;

        //Loop to avoid spawning unit on top of each other
        for (int attempt = 0; attempt < MaxSpawnAttempts; attempt++)
        {
            //Get a random bounds from all associated zones
            int randomIndex = Random.Range(0, _zones.Count);

            //Get random position in the bounds
            float offsetX = Random.Range(-_zones[randomIndex].bounds.extents.x, _zones[randomIndex].bounds.extents.x);
            float offsetZ = Random.Range(-_zones[randomIndex].bounds.extents.z, _zones[randomIndex].bounds.extents.z);
            float offsetY = unit switch
            {
                "Cop" => _copOffsetY,
                "Tank" => _tankOffsetY,
                "Chopper" => _chopperOffsetY,
                _ => 0f,
            };

            Vector3 tentativePosition = _zones[randomIndex].bounds.center + new Vector3(offsetX, offsetY, offsetZ);

            //Check if the position is empty
            Collider[] hitColliders = Physics.OverlapSphere(new Vector3(tentativePosition.x, 0, tentativePosition.z), GetSpawnDetectionRadius(unit), _checkUnitLayer);
            if (hitColliders.Length == 0)
            {
                position = tentativePosition;
                return true;
            }
        }
        return false;
    }


    private float GetSpawnDetectionRadius(string unit)
    {
        return unit switch
        {
            "Cop" => _copSpawnDetectionRadius,
            "Tank" => _tankSpawnDetectionRadius,
            "Chopper" => _chopperSpawnDetectionRadius,
            _ => 1f,
        };
    }
}
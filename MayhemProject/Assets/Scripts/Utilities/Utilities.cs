using UnityEngine;

public class Utilities : MonoBehaviour
{
    #region METHODS
    public static bool IsIntOdd(int value)
    {
        if (value % 2 == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static float DistanceOnXZ(Vector3 pointA, Vector3 pointB)
    {
        float deltaX = pointA.x - pointB.x;
        float deltaZ = pointA.z - pointB.z;

        // Calculate the distance using the Pythagorean theorem
        return Mathf.Sqrt(deltaX * deltaX + deltaZ * deltaZ);
    }
    #endregion

    #region ENUMS
    public enum PlayerBehaviorState
    {
        IDLE, MOVE, DODGE
    }
    
    public enum WeaponType
    {
        PROJECTILE, ZONE, MELEE
    }

    public enum ZonePattern
    {
        LINE, ARC
    }
    #endregion
}

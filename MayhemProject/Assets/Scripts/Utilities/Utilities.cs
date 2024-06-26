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

using UnityEngine;

public static class DebugExtension
{
    public static void DrawSphere(Vector3 position, float radius, Color color, float duration = 0, bool depthTest = true)
    {
        float step = 10f;
        for (float theta = 0; theta < 180; theta += step)
        {
            for (float phi = 0; phi < 360; phi += step)
            {
                Vector3 point = SphericalToCartesian(radius, theta, phi);
                Vector3 nextThetaPoint = SphericalToCartesian(radius, theta + step, phi);
                Vector3 nextPhiPoint = SphericalToCartesian(radius, theta, phi + step);

                Debug.DrawLine(position + point, position + nextThetaPoint, color, duration, depthTest);
                Debug.DrawLine(position + point, position + nextPhiPoint, color, duration, depthTest);
            }
        }
    }

    private static Vector3 SphericalToCartesian(float radius, float theta, float phi)
    {
        float radTheta = Mathf.Deg2Rad * theta;
        float radPhi = Mathf.Deg2Rad * phi;
        float x = radius * Mathf.Sin(radTheta) * Mathf.Cos(radPhi);
        float y = radius * Mathf.Cos(radTheta);
        float z = radius * Mathf.Sin(radTheta) * Mathf.Sin(radPhi);
        return new Vector3(x, y, z);
    }
}

using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PowerLineGenerator : MonoBehaviour
{
    public Transform endPoint; // This will be the child object
    public float slack = 1f; // Controls how much the wire sags
    public int segments = 50;
    public bool autoUpdate = false;

    private void OnDrawGizmos()
    {
        if (endPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.1f);
            Gizmos.DrawWireSphere(endPoint.position, 0.1f);
        }
    }

    public Vector3[] GenerateCatenaryPoints()
    {
        if (endPoint == null) return null;

        Vector3[] points = new Vector3[segments];
        Vector3 localEndPoint = endPoint.localPosition;
        float length = localEndPoint.magnitude;

        // Calculate the parameter 'a' that determines the curve's shape
        float a = slack;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            float x = t * length;  // Goes from 0 to length

            // Catenary formula: y = a * cosh((x - length/2)/a)
            float y = a * (float)System.Math.Cosh((x - length / 2) / a);

            // Offset y to make the curve pass through both points
            float yOffset = a * (float)System.Math.Cosh(length / (2 * a));
            y -= yOffset;

            // Create point in local space
            Vector3 localPoint = new Vector3(x, y, 0);

            // Transform point to align with the direction to the end point
            Quaternion rotation = Quaternion.FromToRotation(Vector3.right, localEndPoint.normalized);
            Vector3 rotatedPoint = rotation * localPoint;

            // Keep everything in local space
            points[i] = rotatedPoint;
        }

        return points;
    }
}
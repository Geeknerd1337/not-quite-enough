using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeckRotateTarget : MonoBehaviour
{
    public Transform target;
    public float speed = 10f;
    public float maxAngle = 30f;
    public float minAngle = -30f;
    public float currentAngle = 0f;
    public float targetAngle = 0f;
    public float rotationSpeed = 10f;

    private Quaternion originalRotation;

    // Start is called before the first frame update
    void Start()
    {
        originalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (target != null)
        {
            // Calculate direction to target in local space
            Vector3 localTarget = transform.InverseTransformPoint(target.position);
            localTarget.y = 0; // Keep rotation only on Y axis

            // Calculate target angle
            targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

            // Clamp the target angle within limits
            targetAngle = Mathf.Clamp(targetAngle, minAngle, maxAngle);
        }
        else
        {
            // Return to original rotation when no target
            targetAngle = 0f;
        }

        // Smoothly interpolate current angle towards target angle
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, Time.deltaTime * rotationSpeed);

        // Apply the rotation while preserving the original rotation
        transform.localRotation = originalRotation * Quaternion.Euler(0, currentAngle, 0);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceWithTopTowardsPlayer : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        if (target == null) return;

        // Get direction to target in world space
        Vector3 direction = target.position - transform.position;

        // Zero out the vertical (Y) component to keep only horizontal direction
        direction.y = 0;

        // If direction is too small, do nothing
        if (direction.sqrMagnitude < 0.001f) return;

        // Create a rotation looking in the horizontal direction (Y axis will face target)
        Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Apply rotation with fixed 90 degrees on X axis
        Vector3 euler = lookRotation.eulerAngles;
        euler.x = 90f; // Fixed X rotation
        euler.z = 0f;  // Optional: keep Z level if needed

        transform.rotation = Quaternion.Euler(euler);
    }
}

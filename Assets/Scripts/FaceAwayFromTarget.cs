using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceAwayFromTarget : MonoBehaviour
{
    public Transform target;

    void Update()
    {
        if (target != null)
        {
            // Make the object face *away* from the target (its -Z toward the target)
            Vector3 directionToTarget = transform.position - target.position;
            if (directionToTarget != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToTarget.normalized);
                transform.rotation = lookRotation;
            }
        }
    }
}


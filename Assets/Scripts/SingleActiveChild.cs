using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleActiveChild : MonoBehaviour
{
    private void Update()
    {
        int activeCount = 0;
        Transform activeChild = null;

        // Count active children and remember the last one seen
        foreach (Transform child in transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeCount++;
                if (activeCount > 1)
                    break;

                activeChild = child;
            }
        }

        // If more than one is active, deactivate all but the last found
        if (activeCount > 1)
        {
            foreach (Transform child in transform)
            {
                if (child != activeChild && child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}

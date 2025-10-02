using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddTimeOnStart : MonoBehaviour
{
    public PhysicalCountdownTimer timer; // Drag your timer here in the Inspector
    public float secondsToAdd = 10f;

    void Start()
    {
        if (timer != null)
        {
            timer.AddTime(secondsToAdd);
        }
    }
}

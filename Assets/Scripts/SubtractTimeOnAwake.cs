using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SubtractTimeOnAwake : MonoBehaviour
{
    public PhysicalCountdownTimer timer;
    public float secondsToSubtract = 10f;

    void OnEnable()
    {
        if (timer != null && timer.timeRemaining > 10f)
        {
            float maxSubtractable = timer.timeRemaining - 10f;
            float actualSubtract = Mathf.Min(secondsToSubtract, maxSubtractable);
            timer.AddTime(-actualSubtract);
        }
    }
}



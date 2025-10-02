using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableAfterAmountTime : MonoBehaviour
{
    public float amount = 1.3f;
    // Start is called before the first frame update
    void OnEnable()
    {
        amount = 1.3f;
    }

    void Update()
    {
        amount -= Time.deltaTime;

        if (amount <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}

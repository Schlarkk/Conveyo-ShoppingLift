using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrader : MonoBehaviour
{
    public float UpgradeValue = 1.2f;

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Poop"))
        {
            ItemValue iv = other.gameObject.GetComponent<ItemValue>();
            if (iv != null)
            {
                iv.Value = iv.Value * UpgradeValue;
            }
        }
    }
}

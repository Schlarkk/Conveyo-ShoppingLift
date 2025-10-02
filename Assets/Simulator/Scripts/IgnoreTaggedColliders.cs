using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnoreTaggedColliders : MonoBehaviour
{
    public string tagToIgnore = "IgnoreMe"; // Set this to the tag you want to ignore

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(tagToIgnore))
        {
            Physics.IgnoreCollision(GetComponent<Collider>(), collision.collider);
        }
    }
}



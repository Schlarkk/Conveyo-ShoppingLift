using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourMachine : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.back; // Direction relative to the conveyor's local space
    public float speed = 2f; // Speed of the belt

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            // Convert local moveDirection to world space
            Vector3 worldMoveDir = transform.TransformDirection(moveDirection.normalized);

            // Move the object along the belt smoothly in world space
            Vector3 move = worldMoveDir * speed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Renderer rend = other.GetComponent<Renderer>();
        ItemValue iv = other.gameObject.GetComponent<ItemValue>();
        if (rend != null)
        {
            // Generate a random color and apply it to the material
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            rend.material.color = randomColor;
            iv.Value++;
        }
    }
}

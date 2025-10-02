using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Splitter : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.back; // Direction relative to the conveyor's local space
    public float speed = 2f; // Speed of the belt
    public float reverseInterval = 2f; // How often to reverse direction

    private float timer = 0f;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= reverseInterval)
        {
            moveDirection = -moveDirection; // Flip the direction
            timer = 0f;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null && !rb.isKinematic)
        {
            Vector3 worldMoveDir = transform.TransformDirection(moveDirection.normalized);
            Vector3 move = worldMoveDir * speed * Time.deltaTime;
            rb.MovePosition(rb.position + move);
        }
    }
}

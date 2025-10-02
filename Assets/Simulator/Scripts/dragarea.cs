using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dragarea : MonoBehaviour
{
    public float speed = 5f;          // Speed of vertical movement

    private bool isDragging = false;
    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            float mouseY = Input.GetAxis("Mouse Y");
            Vector3 localPos = transform.localPosition;

            // Move along local Y based on mouse movement
            localPos.y += mouseY * speed * Time.deltaTime;

            // Clamp local Y between startLocalPos.y (0) and startLocalPos.y + 0.8
            localPos.y = Mathf.Clamp(localPos.y, startLocalPos.y, startLocalPos.y + 0.8f);

            transform.localPosition = localPos;
        }
    }
}


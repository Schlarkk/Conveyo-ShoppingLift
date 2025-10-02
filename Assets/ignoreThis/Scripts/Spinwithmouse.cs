using UnityEngine;

public class Spinwithmouse : MonoBehaviour
{
    [Header("Settings")]
    public float rotateSpeed = 5f; // Sensitivity of rotation

    private bool isHolding = false;
    private Vector3 lastMousePos;

    void Update()
    {
        // Detect if M1 is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            isHolding = true;
            lastMousePos = Input.mousePosition;
        }

        // Detect if M1 is released
        if (Input.GetMouseButtonUp(0))
        {
            isHolding = false;
        }

        // Rotate while holding M1
        if (isHolding)
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            lastMousePos = Input.mousePosition;

            // Rotate around local axes
            float rotationX = -delta.y * rotateSpeed * Time.deltaTime;
            float rotationY = delta.x * rotateSpeed * Time.deltaTime;

            transform.Rotate(Vector3.up, rotationY, Space.World);
            transform.Rotate(Vector3.right, rotationX, Space.World);
        }
    }
}

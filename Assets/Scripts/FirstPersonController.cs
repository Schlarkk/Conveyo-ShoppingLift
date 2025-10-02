using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float crouchSpeed = 3f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("Look")]
    public Transform playerCamera;
    public float mouseSensitivity = 100f;
    public float lookUpClamp = 80f;

    [Header("Crouch")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public float crouchTransitionSpeed = 6f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float xRotation = 0f;
    public bool isCrouching = false;
    private float currentSpeed;

    private float targetHeight;
    private Vector3 initialCameraLocalPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        targetHeight = standingHeight;
        controller.height = standingHeight;
        controller.center = new Vector3(0, standingHeight / 2f, 0);
        initialCameraLocalPos = playerCamera.localPosition;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleCrouchInput();
        HandleMovement();
        HandleLook();
        SmoothCrouchTransition();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            // Optional: reduce jump height when crouching
            float jumpPower = isCrouching ? jumpHeight * 0.7f : jumpHeight;
            velocity.y = Mathf.Sqrt(jumpPower * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -lookUpClamp, lookUpClamp);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouchInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            currentSpeed = crouchSpeed;
            targetHeight = crouchingHeight;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            currentSpeed = walkSpeed;
            targetHeight = standingHeight;
        }
    }

    void SmoothCrouchTransition()
    {
        float currentHeight = controller.height;
        float newHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);

        controller.height = newHeight;
        controller.center = new Vector3(0, newHeight / 2f, 0);

        // Camera height also lerps
        Vector3 camPos = playerCamera.localPosition;
        float targetCamY = newHeight - 0.5f;
        camPos.y = Mathf.Lerp(camPos.y, targetCamY, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.localPosition = camPos;
    }

    public bool IsCrouching()
    {
        return isCrouching;
    }
}

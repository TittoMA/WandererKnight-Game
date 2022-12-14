using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float walkSpeed = 5;

    [SerializeField] private float runSpeed = 8;

    [SerializeField] private float rotationSpeed = 720;

    [SerializeField] private float jumpSpeed = 5;

    [SerializeField] private float jumpButtonGracePeriod = 0.2f;

    [SerializeField] private Transform cameraTransform;
    [SerializeField] private AudioSource footStepSfx;

    private PlayerInputActions playerControls;
    private Animator animator;
    private CharacterController characterController;

    private InputAction move;
    private InputAction jump;
    private InputAction roll;
    private InputAction sprint;

    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private bool isJumping;
    private bool isGrounded;
    private Vector3 movementDirection;

    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();

        sprint = playerControls.Player.Sprint;
        sprint.Enable();

        jump = playerControls.Player.Jump;
        jump.Enable();
        jump.performed += Jump;

        roll = playerControls.Player.Roll;
        roll.Enable();
        roll.performed += Roll;
    }

    private void OnDisable()
    {
        move.Disable();
        sprint.Disable();

        jump.performed -= Jump;
        jump.Disable();

        roll.performed -= Roll;
        roll.Disable();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 inputMove = move.ReadValue<Vector2>();

        movementDirection = new Vector3(inputMove.x, 0f, inputMove.y);
        float inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);

        float speed = inputMagnitude * walkSpeed;
        if (sprint.ReadValue<float>() > 0)
        {
            speed = inputMagnitude * runSpeed;
        }
        else
        {
            inputMagnitude /= 2;
        }

        animator.SetFloat("Input Magnitude", inputMagnitude, 0.05f, Time.deltaTime);
        movementDirection = Quaternion.AngleAxis(cameraTransform.rotation.eulerAngles.y, Vector3.up) * movementDirection;
        movementDirection.Normalize();

        ySpeed += -9.8f * Time.deltaTime;

        Vector3 velocity = movementDirection * speed;
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);

        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
            animator.SetBool("isGrounded", true);
            isGrounded = true;
        }

        if (Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;
            animator.SetBool("isJumping", false);
            isJumping = false;
            animator.SetBool("isFalling", false);

            if (Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                animator.SetBool("isJumping", true);
                isJumping = true;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0;
            animator.SetBool("isGrounded", false);
            isGrounded = false;

            if ((isJumping && ySpeed < 0) || ySpeed < -2.5f)
            {
                animator.SetBool("isFalling", true);
            }
        }

        if (movementDirection != Vector3.zero)
        {
            animator.SetBool("isMoving", true);


            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            footStepSfx.Stop();
            animator.SetBool("isMoving", false);
        }

    }

    private void Jump(InputAction.CallbackContext context)
    {
        jumpButtonPressedTime = Time.time;
    }

    private void Roll(InputAction.CallbackContext context)
    {
        if (movementDirection != Vector3.zero)
        {
            animator.SetTrigger("doRoll");
        }
    }

    public void WalkSound(AnimationEvent evt)
    {
        if (evt.animatorClipInfo.weight >= 0.5)
        {
            footStepSfx.Play();
        }
    }
}

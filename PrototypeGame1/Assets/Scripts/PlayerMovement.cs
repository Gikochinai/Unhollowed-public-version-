using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jump Movement")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    private bool readyToJump = true;
    //private bool isBufferingGroundCheck = false;
    //private float lastGroundedTime;
    //public float jumpBufferDuration = 0.2f; // Time to wait after hitting the ground before allowing a jump


    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Ground Check")]
    public float playerHeight; 
    public LayerMask whatIsGround;
    private bool grounded;

    public Transform orientation;
    private float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    private Rigidbody rb;
    //public BoxCollider boxCollider;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        air
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void GroundCheck()
    {
        float groundCheckDistance = playerHeight / 2 + 0.1f; // Adjust distance as needed
        Vector3 groundCheckPosition = transform.position + Vector3.up * (playerHeight / 2 - 0.1f); // Check from the chest or head down

        // Perform the ground check with a Raycast from the upper body to the feet
        grounded = Physics.Raycast(groundCheckPosition, Vector3.down, groundCheckDistance, whatIsGround);

        // Debugging: Visualize the ground check
        Debug.DrawLine(groundCheckPosition, groundCheckPosition + Vector3.down * groundCheckDistance, grounded ? Color.green : Color.red);
        //Debug.Log("Grounded: " + grounded); // Log for debugging
    }








    private void Update()
    {
        GroundCheck(); // Perform ground check each frame
        MyInput();     // Read input and possibly jump
        SpeedControl();
        StateHandler();

        // Adjust drag based on whether grounded
        rb.drag = grounded ? groundDrag : 0;

        // Only reset jump readiness if grounded after cooldown
        if (grounded && !readyToJump)
        {
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // Log state for debugging
        //Debug.Log($"Grounded: {grounded}, On Slope: {OnSlope()}, Can Jump: readyToJump[{readyToJump}] && grounded[{grounded}] = {readyToJump && grounded}");

    }



    private void StateHandler()
    {
        if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }
        else
        {
            state = MovementState.air;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Only allow jumping if the player is grounded and ready to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            Jump();
        }
    }


    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else // Only apply downward force if not grounded
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

            // Apply a downward force to ensure the player falls
            //rb.AddForce(Vector3.down * 10f, ForceMode.Force); // Adjust the value as needed
        }
        rb.useGravity = !OnSlope();
    }


    /*private void EndGroundCheckBuffer()
    {
        isBufferingGroundCheck = false;
    }*/


    private void Jump()
    {
        exitingSlope = true;
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z); // Reset y velocity

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        readyToJump = false;
        //grounded = false; // Ensure grounded is false immediately on jump
    }



    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)) {
            float anlge = Vector3.Angle(Vector3.up, slopeHit.normal);
            return anlge < maxSlopeAngle && anlge != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    private void SpeedControl()
    {
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
                rb.velocity = rb.velocity.normalized * moveSpeed;
            }
        }
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }
}

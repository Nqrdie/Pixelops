using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement variables")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float gunWeight;
    [SerializeField] private float MaxAirSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    private InputHandler inputHandler;

    private Rigidbody rb;

    private void Start()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = gameObject.GetComponent<Rigidbody>();
        inputHandler = FindFirstObjectByType<InputHandler>();

        rb.freezeRotation = true;
        readyToJump = true;

        Camera.main.GetComponent<PlayerCam>().player = transform;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        if (inputHandler.jumpTriggered && readyToJump && grounded)
        {
            readyToJump = false; 
            Jump();
        }


        SpeedControl();

        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Calculate movement speed based on if the player is sprinting
        float speed = walkSpeed; //* (holdingKnife ? gunWeight : 1f);

        // Movement input from player
        Vector3 moveDirection = transform.forward * inputHandler.moveInput.y + transform.right * inputHandler.moveInput.x;

        if(grounded)
            rb.AddForce(moveDirection * speed, ForceMode.Force);
        else if(!grounded)
            rb.AddForce(moveDirection * speed * airMultiplier, ForceMode.Force);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > walkSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * walkSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

}

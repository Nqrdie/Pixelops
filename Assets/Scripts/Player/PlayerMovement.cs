using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement variables")]
    [SerializeField] private float walkSpeed;
    [SerializeField] private float gunWeight;
    [SerializeField] private float MaxAirSpeed;
    [SerializeField] private GameObject[] playerModels;

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

        rb = gameObject.GetComponentInParent<Rigidbody>();
        inputHandler = FindFirstObjectByType<InputHandler>();

        rb.freezeRotation = true;
        readyToJump = true;

        Camera.main.GetComponent<PlayerCam>().player = transform;

        playerModels[0].SetActive(false);
        playerModels[1].SetActive(false);
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
        float speed = walkSpeed;
        Vector3 moveDirection = transform.forward * inputHandler.moveInput.y + transform.right * inputHandler.moveInput.x;

        if (grounded)
        {
            rb.AddForce(moveDirection * speed, ForceMode.Force);
        }
        else
        {
            // i hate this airstrafe bs
            Vector3 velocity = rb.linearVelocity;
            Vector3 flatVelocity = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 desiredDirection = moveDirection.normalized;

            if (inputHandler.moveInput.magnitude > 0)
            {
                Vector3 airControlForce = desiredDirection * speed * airMultiplier;
                rb.AddForce(airControlForce, ForceMode.Acceleration);
            }
            if (flatVelocity.magnitude > MaxAirSpeed)
            {
                flatVelocity = flatVelocity.normalized * MaxAirSpeed;
                rb.linearVelocity = new Vector3(flatVelocity.x, rb.linearVelocity.y, flatVelocity.z);
            }
        }
    }
    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJump), jumpCooldown);

        GetComponent<PlayerTeamManager>().enabled = true;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

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

using UnityEngine;

/// <summary>
/// Basic player controller script. Add movement and interaction logic as needed.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 5f;
    public float jetpackThrust = 15f;
    public float maxFallSpeed = 20f;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool isGrounded;
    private bool isJumping;
    private bool isCrouching;

    private bool isNoclip = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        CheckGrounded();
        HandleCrouch();
        HandleInput();
        HandleNoclip();
    }

    private void CheckGrounded()
    {
        float checkDistance = (capsule != null ? capsule.height / 2f : 1f) + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance);
        if (isGrounded) isJumping = false;
    }

    private void HandleNoclip()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isNoclip = !isNoclip;
            rb.detectCollisions = !isNoclip;
            rb.useGravity = !isNoclip;
            rb.velocity = Vector3.zero;
            if (capsule != null) capsule.enabled = !isNoclip;
        }
    }

    private void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            if (capsule != null) capsule.height = crouchHeight;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            if (capsule != null) capsule.height = standHeight;
        }
    }

    private void HandleInput()
    {
        float speed = isCrouching ? crouchSpeed : moveSpeed;

        // Basic movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // In noclip mode, move in camera direction including vertical
        if (isNoclip)
        {
            Transform cam = transform.GetComponentInChildren<Camera>()?.transform;
            Vector3 dir = cam != null
                ? cam.TransformDirection(new Vector3(horizontal, 0, vertical))
                : transform.TransformDirection(new Vector3(horizontal, 0, vertical));

            // Space = fly up, Left Ctrl = fly down
            float verticalInput = 0f;
            if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
            else if (Input.GetKey(KeyCode.LeftControl)) verticalInput = -1f;

            dir += Vector3.up * verticalInput;
            rb.velocity = dir * speed * 2f;
            return;
        }
        
        Vector3 movement = transform.TransformDirection(new Vector3(horizontal, 0, vertical)) * speed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        // Jump button press - initial jump (not while crouching)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = true;
        }
        
        // Hold space - jetpack thrust (works anytime you're airborne)
        if (Input.GetKey(KeyCode.Space) && !isGrounded)
        {
            rb.AddForce(Vector3.up * jetpackThrust, ForceMode.Force);
            
            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
            }
        }
        
        // Jump button released - stop jetpack
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isJumping = false;
        }
    }


}

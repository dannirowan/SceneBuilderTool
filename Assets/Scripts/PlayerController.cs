using UnityEngine;

/// <summary>
/// Basic player controller script. Add movement and interaction logic as needed.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpForce = 3.5f;
    public float jetpackThrust = 15f;
    public float maxFallSpeed = 20f;
    public float standHeight = 2f;
    public float crouchHeight = 1f;
    public float interactionDistance = 3f;
    public bool showReticle = true;
    
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private bool isGrounded;
    private bool isCrouching;

    private bool isNoclip = false;
    private Camera playerCamera;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        CheckGrounded();
        HandleCrouch();
        HandleInput();
        HandleNoclip();
        HandleInteraction();
    }

    private void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E) || playerCamera == null)
        {
            return;
        }

        Ray interactionRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(interactionRay, out RaycastHit hit, interactionDistance))
        {
            LiftButton liftButton = hit.collider.GetComponent<LiftButton>() ?? hit.collider.GetComponentInParent<LiftButton>();
            if (liftButton != null)
            {
                liftButton.PressButton();
            }
        }
    }

    private void OnGUI()
    {
        if (!showReticle || playerCamera == null)
        {
            return;
        }

        const float reticleSize = 4f;
        float reticleX = (Screen.width - reticleSize) * 0.5f;
        float reticleY = (Screen.height - reticleSize) * 0.5f;
        GUI.Box(new Rect(reticleX, reticleY, reticleSize, reticleSize), string.Empty);
    }

    private void CheckGrounded()
    {
        float checkDistance = (capsule != null ? capsule.height / 2f : 1f) + 0.1f;
        isGrounded = Physics.Raycast(transform.position, Vector3.down, checkDistance);
    }

    private void HandleNoclip()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isNoclip = !isNoclip;
            rb.detectCollisions = !isNoclip;
            rb.useGravity = !isNoclip;
            rb.velocity = Vector3.zero;
            if (isNoclip)
            {
                isCrouching = false;
            }
            if (capsule != null) capsule.enabled = !isNoclip;
        }
    }

    private void HandleCrouch()
    {
        if (isNoclip)
        {
            return;
        }

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
        float speed = isNoclip ? moveSpeed : (isCrouching ? crouchSpeed : moveSpeed);

        // Basic movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // In noclip mode, move in camera direction including vertical
        if (isNoclip)
        {
            Transform cam = transform.GetComponentInChildren<Camera>()?.transform;
            Vector3 horizontalDir = cam != null
                ? cam.TransformDirection(new Vector3(horizontal, 0, vertical))
                : transform.TransformDirection(new Vector3(horizontal, 0, vertical));

            float verticalVelocity = 0f;
            if (Input.GetKey(KeyCode.Space)) verticalVelocity = jetpackThrust;
            else if (Input.GetKey(KeyCode.LeftControl)) verticalVelocity = -jetpackThrust;

            Vector3 horizontalVelocity = horizontalDir * speed * 2f;
            rb.velocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.z);
            return;
        }
        
        Vector3 movement = transform.TransformDirection(new Vector3(horizontal, 0, vertical)) * speed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        // Jump button press - initial jump (not while crouching)
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isCrouching)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
        
        // Clamp downward speed while falling.
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector3(rb.velocity.x, -maxFallSpeed, rb.velocity.z);
        }
        
    }


}

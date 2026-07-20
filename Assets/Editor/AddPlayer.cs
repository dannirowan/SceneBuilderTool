using UnityEngine;
using UnityEditor;

public class AddPlayer
{
    [MenuItem("Tools/Add Player to Scene")]
    public static void CreatePlayer()
    {
        // Create a new GameObject for the player
        GameObject player = new GameObject("Player");
        
        // Add a capsule collider (common for player characters)
        CapsuleCollider collider = player.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.5f;
        
        // Add a rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.mass = 1f;
        rb.drag = 0f;
        rb.angularDrag = 0.05f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        // Add a basic player controller script (placeholder)
        player.AddComponent<PlayerController>();
        
        // Register with undo system
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        
        // Select the player in the hierarchy
        Selection.activeGameObject = player;
        
        Debug.Log("Player created with collider and controller script.");
    }
}

/// <summary>
/// Basic player controller script. Add movement and interaction logic as needed.
/// </summary>
public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    
    private Rigidbody rb;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Basic movement input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed;
        rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        
        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground") || Vector3.Dot(collision.relativeVelocity, Vector3.up) < 0)
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }
}

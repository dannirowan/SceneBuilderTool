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
        
        // Add a basic player controller script
        player.AddComponent<PlayerController>();
        
        // Register with undo system
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        
        // Select the player in the hierarchy
        Selection.activeGameObject = player;
        
        Debug.Log("Player created with collider and controller script.");
    }
}

using UnityEngine;
using UnityEditor;

public class AddPlayer
{
    [MenuItem("Tools/Add Player to Scene")]
    public static void CreatePlayer()
    {
        // Disable any existing Main Camera in the scene
        Camera existingCam = Camera.main;
        if (existingCam != null)
        {
            Undo.RecordObject(existingCam.gameObject, "Disable Existing Camera");
            existingCam.gameObject.SetActive(false);
        }

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
        
        // Create first-person camera as child of player
        GameObject camObject = new GameObject("PlayerCamera");
        camObject.transform.SetParent(player.transform);
        camObject.transform.localPosition = new Vector3(0f, 0.75f, 0f); // Eye level
        camObject.transform.localRotation = Quaternion.identity;
        Camera cam = camObject.AddComponent<Camera>();
        cam.tag = "MainCamera";
        camObject.AddComponent<AudioListener>();
        camObject.AddComponent<PlayerCamera>();
        Undo.RegisterCreatedObjectUndo(camObject, "Create Player Camera");
        
        // Register with undo system
        Undo.RegisterCreatedObjectUndo(player, "Create Player");
        
        // Select the player in the hierarchy
        Selection.activeGameObject = player;
        
        Debug.Log("Player created with collider and controller script.");
    }
}

using UnityEngine;

/// <summary>
/// First-person camera controller. Attach to the Camera child of the Player.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;

    private Transform playerBody;
    private float xRotation = 0f;

    private void Start()
    {
        playerBody = transform.parent;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Tilt camera up/down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate player body left/right
        playerBody.Rotate(Vector3.up * mouseX);
        
        // Unlock cursor on Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

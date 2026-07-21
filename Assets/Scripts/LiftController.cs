using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    [Header("Lift Settings")]
    public float moveSpeed = 3.0f;
    public int currentFloorIndex = 0;
    
    [Header("Floor Data")]
    public List<float> floorYPositions = new List<float>();

    private bool isMoving = false;
    private float targetY;

    private void Start()
    {
        // Ensure lift starts at the ground floor (index 0)
        if (floorYPositions.Count > 0)
        {
            Vector3 pos = transform.position;
            pos.y = floorYPositions[0];
            transform.position = pos;
            targetY = pos.y;
        }
    }

    private void Update()
    {
        if (isMoving)
        {
            Vector3 targetPosition = new Vector3(transform.position.x, targetY, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

            if (Mathf.Approximately(transform.position.y, targetY))
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    public void MoveToFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= floorYPositions.Count) return;
        
        currentFloorIndex = floorIndex;
        targetY = floorYPositions[floorIndex];
        isMoving = true;
    }
}

/// <summary>
/// Clickable or Raycastable button attached to the panel inside the lift.
/// </summary>
public class LiftButton : MonoBehaviour
{
    public LiftController controller;
    public int targetFloorIndex;

    // Direct click support (standard Unity collider detection)
    private void OnMouseDown()
    {
        PressButton();
    }

    // Call this if using standard FPS Raycasting or Interaction toolkits
    public void PressButton()
    {
        if (controller != null)
        {
            controller.MoveToFloor(targetFloorIndex);
        }
    }
}
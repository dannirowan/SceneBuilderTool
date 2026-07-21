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
    // Use current scene Y position as the starting point
    targetY = transform.position.y;

    // If floorYPositions has values, assign current Y to floor 0 if empty
    if (floorYPositions.Count > 0)
    {
        currentFloorIndex = 0;
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
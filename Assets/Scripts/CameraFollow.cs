using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Reference to the player's transform
    public float currentDistance = 5f;  // Initial/current distance from the player
    public float minDistance = 2f;  // Minimum distance
    public float maxDistance = 10f;  // Maximum distance
    public float scrollSpeed = 2f;  // Scrolling zoom speed
    public float rotationSpeed = 75f;  // Rotating speed
    public float angleX = 35f;  // Fixed X-rotation angle of camera
    public float currentRotationY = 0f;  // Current Y-rotation axis of camera

    void Update()
    {
        if(player != null) 
        {
            // Update camera position and rotation
            Quaternion rotation = Quaternion.Euler(angleX, currentRotationY, 0);
            Vector3 offset = new Vector3(0, 0, -currentDistance);
            transform.position = player.position + rotation * offset;

            // Make the camera always face the player
            transform.LookAt(player);
        }
    }

    // Update camera rotation value
    public void Rotate(float rotateInput) 
    {
        currentRotationY += rotateInput * rotationSpeed * Time.deltaTime;
    }

    // Update camera zoom value
    public void Zoom(float scrollInput) 
    {
        currentDistance -= scrollInput * scrollSpeed;
        currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);
    }
}

// File: _Scripts/Gameplay/CameraController.cs

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float smoothSpeed = 0.125f; // How fast the camera follows

    private Transform target;
    private Vector3 offset;
    private Vector3 initialPosition;

    void Start()
    {
        // Store the initial position and offset from the world origin (or a start point)
        initialPosition = transform.position;
        offset = transform.position; // Assuming camera starts in its ideal offset position
    }

    // LateUpdate is called after all Update functions. Best for camera movement.
    void LateUpdate()
    {
        if (target == null) return;

        // Define the desired position: target's position plus the initial offset
        Vector3 desiredPosition = new Vector3(target.position.x, 0, target.position.z) + offset;
        
        // Smoothly interpolate from the camera's current position to the desired position
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Sets a new domino for the camera to follow.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Resets the camera to its original starting position.
    /// </summary>
    public void ResetPosition()
    {
        target = null; // Stop following
        transform.position = initialPosition;
    }
}
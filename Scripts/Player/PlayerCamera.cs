using UnityEngine;

/// <summary>
/// Controls player camera movement, including mouse look, locking, and resetting.
/// </summary>
public class PlayerCamera : MonoBehaviour
{
    [Tooltip("Horizontal mouse sensitivity")]
    [SerializeField] private float sensX;
    
    [Tooltip("Vertical mouse sensitivity")]
    [SerializeField] private float sensY;

    [Tooltip("Reference to player orientation transform")]
    public Transform orientation;

    // Current camera rotation values
    public float xRotation;
    public float yRotation;

    [Tooltip("Starting Y rotation for camera reset")]
    public float initialYRotation;
    
    // Prevents mouse input when true
    private bool isLocked = false;

    /// <summary>
    /// Initialize camera and cursor settings.
    /// </summary>
    private void Start()
    { 
        ResetCamera();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Process mouse input and handle camera rotation.
    /// </summary>
    private void Update()
    {
        // Skip all input handling when camera is locked
        if (isLocked)
            return;
        
        // Calculate mouse movement with sensitivity applied
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        // Update rotation values
        yRotation += mouseX;
        xRotation -= mouseY;
        
        // Limit vertical rotation to prevent over-rotation
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the camera
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Rotate the player body to match camera's horizontal rotation
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);

        // Reset camera if reset input detected
        if (Input.GetButton("F"))
        {
            ResetCamera();
        }
    }
    
    /// <summary>
    /// Locks the camera and forces it to a specific pitch.
    /// The current yaw is maintained.
    /// </summary>
    /// <param name="pitch">The pitch angle in degrees to set</param>
    public void LockCameraAndSetPitch(float pitch)
    {
        isLocked = true;
        xRotation = pitch;
        
        // Force apply the rotation immediately, maintaining current yaw
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);
    }
    
    /// <summary>
    /// Unlocks the camera, allowing mouse input to resume.
    /// </summary>
    public void UnlockCamera()
    {
        isLocked = false;
    }

    /// <summary>
    /// Resets camera to initial position and rotation.
    /// </summary>
    public void ResetCamera()
    {
        yRotation = initialYRotation;
        xRotation = 0f;
        orientation.rotation = Quaternion.Euler(0f, initialYRotation, 0f);
        transform.localRotation = Quaternion.Euler(0f, initialYRotation, 0f);
    }
}
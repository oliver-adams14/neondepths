using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


// This code was made by following a tutorial 
//Dave. (2023). FIRST PERSON MOVEMENT in 10 MINUTES - Unity Tutorial. [Online]. Youtube. Last Updated: 2023. Available at: https://www.youtube.com/watch?v=f473C43s8nE [Accessed 29 December 2023].
//START

// Make sure to create a layer named "Wall" and assign it to your wall objects and the 'What Is Wall' field in the Inspector.
public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]

    [SerializeField] private ParticleSystem movementParticles;
    public float moveSpeed;
    private float origionalMoveSpeed;
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [SerializeField] private float jumpMultiplier = 1.5f;
    [SerializeField] private float fallMultiplier = 2.5f;   

    [Header("Sliding")]
    public float slideForce = 200f; // Initial burst force
    public float slideYScale = 0.5f;
    public float minSlideSpeed = 2f;
    public float maxSlideSpeed = 10f; // Max speed cap
    public float slideSlopeMultiplier = 1.5f; // Multiplier for sliding down slopes
    private float startYScale;
    private float slideDuration = 0.5f; // How long the initial burst lasts on flat ground

    [Header("Wallriding")]
    public float wallRunSpeed = 8f;
    public float wallRunSlideSpeed = 2f; // Renamed from wallRunSlideDownForce for clarity
    public float wallJumpUpForce = 10f;
    public float wallJumpSideForce = 8f;
    public float wallStickForce = 100f;
    public float wallCheckDistance = 1f;
    public LayerMask whatIsWall;
    [Header("Wall Run")]
    public float wallrunForce, maxWallrunTime, maxWallrunSpeed;
    public float wallClimbSpeed;
    public float wallClimbJumpForce = 15f;
    public float maxWallClimbTime = 5f;
    public bool isWallRunning;
    public float wallrunTimer;
    [Header("Camera Effects")]
    public Transform cameraTransform; // Assign your player camera transform in the inspector
    public PlayerCamera playerCamera; // Reference to the camera script
    public float wallRunCameraTilt = 45f;
    public float wallClimbCameraPitch = -20f; // The upward pitch of the camera during wall climb
    private float currentCameraTilt = 0f;
    private float targetCameraTilt = 0f;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode slideKey = KeyCode.LeftControl;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;

    [SerializeField] private GameObject startScreen;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    // Sliding
    private bool sliding;
    private bool isSliding;
    private float slideStartTime;
    private float originalDrag;

    // Wallriding
    private bool wallRiding;
    private bool wallLeft;
    private bool wallRight;
    private RaycastHit leftWallhit;    private RaycastHit rightWallhit;
    private float targetTilt; // For camera tilt
    private Quaternion originalCameraRotation;

    // Wall Climbing
    private bool isClimbing;
    private bool wallClimbModeActive;
    private Vector3 wallClimbNormal;
    private Coroutine slideCoroutine;

    private void Start()
    {
        origionalMoveSpeed = moveSpeed;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        startYScale = transform.localScale.y;        if (cameraTransform == null)
        {
            Debug.LogError("PlayerMovement: Camera Transform is not assigned in the inspector!");
        }
        else
        {
            // Store the original camera rotation to preserve mouse look
            originalCameraRotation = cameraTransform.localRotation;
        }
    }
    private void speedIncrease()
    {
        moveSpeed =  moveSpeed * 1.5f;
    }

    private void speedDecrease()
    {
        moveSpeed = origionalMoveSpeed;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);        // Reset state if T is pressed (changed from G to avoid confusion, F is now used for level reset)
        if (Input.GetKeyDown(KeyCode.T))
        {
            ResetPlayerState();
        }

        MyInput();
        SpeedControl();
        CheckForWall();

        // handle drag
        if (grounded && !isClimbing)
        {
            rb.linearDamping = groundDrag;
            if (wallRiding) StopWallRide();
        }
        else if (!isClimbing)
        {
            rb.linearDamping = 0;
        }

        // Sliding Input
        if (Input.GetKeyDown(slideKey) && grounded)
            StartSlide();

        if (Input.GetKeyUp(slideKey) && sliding)
            StopSlide();
        
        // Stop slide if speed is too low
        if (sliding && rb.linearVelocity.magnitude < minSlideSpeed && grounded)
            StopSlide();        // Wallride Input
        HandleWallRidingInput();
    }

    private void LateUpdate()
    {
        // Handle camera tilt in LateUpdate to avoid conflicts with mouse look
        UpdateCameraTilt();
    }

    private void FixedUpdate()
    {
        if (isClimbing)
            WallClimbingMovement();
        else if (wallRiding)
            WallRunning();
        else if (sliding)
            SlidingMovement();
        else
            MovePlayer();

        // Apply additional downward force when falling to speed up descent.
        if (!grounded && rb.linearVelocity.y < 0 && !isClimbing)
        {
            rb.AddForce(Vector3.down * (fallMultiplier - 1) * Mathf.Abs(Physics.gravity.y), ForceMode.Acceleration);
        }
        
        // Jump is handled in FixedUpdate if needed:
        if (Input.GetKey(jumpKey) && readyToJump && grounded && !isClimbing)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump

    }

    private void MovePlayer()
    {
        if (isClimbing) return; // Don't move normally while climbing

        // Get movement input (ensure these variables are declared in your script)
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        // Calculate movement direction
        moveDirection = orientation.TransformDirection(Vector3.forward) * verticalInput + orientation.TransformDirection(Vector3.right) * horizontalInput;

        // Play particle system when moving, stop when idle
        if (horizontalInput != 0 || verticalInput != 0)
    {
        if (!movementParticles.isPlaying)
            movementParticles.Play();
    }
    else
    {
        if (movementParticles.isPlaying)
            movementParticles.Stop();
    }

    // Apply movement force
    if (grounded)
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    else
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
}

    private void SpeedControl()
    {
        // Don't control speed during special movements
        if (wallRiding || sliding || isClimbing) return;

        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        float currentMoveSpeed = moveSpeed;

        // limit velocity if needed
        if (flatVel.magnitude > currentMoveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * currentMoveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // Reset vertical velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        // Apply jump force with multiplier
        rb.AddForce(transform.up * jumpForce * jumpMultiplier, ForceMode.Impulse);
    }
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void StartSlide()
    {
        sliding = true;
        slideStartTime = Time.time;
        transform.localScale = new Vector3(transform.localScale.x, slideYScale, transform.localScale.z);
        // Store original drag
        originalDrag = rb.linearDamping;
    }    private void SlidingMovement()
    {
        // Use orientation's forward direction
        moveDirection = orientation.forward;
        
        // Multiple raycasts for better ground detection
        Vector3 rayOrigin = transform.position;
        Vector3 forwardRayOrigin = rayOrigin + moveDirection * 0.3f;
        
        // Get surface normal under player with multiple checks
        RaycastHit hit;
        bool onSurface = false;
        
        // Try forward-projected raycast first
        if (Physics.Raycast(forwardRayOrigin, Vector3.down, out hit, playerHeight * 0.5f + 1f, whatIsGround))
        {
            onSurface = true;
        }
        // Fallback to direct downward raycast
        else if (Physics.Raycast(rayOrigin, Vector3.down, out hit, playerHeight * 0.5f + 1f, whatIsGround))
        {
            onSurface = true;
        }
        // Try a bit behind the player too
        else if (Physics.Raycast(rayOrigin - moveDirection * 0.2f, Vector3.down, out hit, playerHeight * 0.5f + 1f, whatIsGround))
        {
            onSurface = true;
        }

        // Calculate slope angle and direction
        float slopeAngle = 0f;
        Vector3 slopeDirection = moveDirection;
        
        if (onSurface)
        {
            // Calculate slope angle
            slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            
            // Determine if we're going downhill
            Vector3 slopeVector = Vector3.ProjectOnPlane(moveDirection, hit.normal).normalized;
            float slopeDot = Vector3.Dot(slopeVector, Vector3.down);
            
            if (slopeDot > 0.1f) // Going downhill
            {
                slopeAngle = -slopeAngle; // Make downhill negative
                slopeDirection = slopeVector;
            }
            else if (slopeDot < -0.1f) // Going uphill
            {
                slopeDirection = slopeVector;
            }
        }
        
        // Check how long we've been sliding
        float elapsedSlideTime = Time.time - slideStartTime;
        
        // Check current velocity
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        
        // On downhill slopes, accelerate
        if (slopeAngle < -10f) // Steeper downhill threshold
        {
            // Apply sliding force based on slope steepness along the slope direction
            float slopeForce = slideForce * (1f + Mathf.Abs(slopeAngle/90f) * slideSlopeMultiplier);
            
            // Apply force if below max speed
            if (flatVel.magnitude < maxSlideSpeed)
            {
                rb.AddForce(slopeDirection * slopeForce, ForceMode.Force);
            }
            
            // Reduce drag on slopes to maintain momentum
            rb.linearDamping = originalDrag * 0.3f;
        }
        // On flat/uphill, slow down after initial burst
        else
        {
            // Initial burst of speed
            if (elapsedSlideTime < slideDuration)
            {
                float remainingFactor = 1f - (elapsedSlideTime / slideDuration);
                rb.AddForce(moveDirection.normalized * slideForce * remainingFactor, ForceMode.Force);
                rb.linearDamping = originalDrag * 0.5f;
            }
            // Otherwise increase drag to slow down
            else
            {
                rb.linearDamping = originalDrag * 2.5f;
            }
            
            // Automatically end slide on flat/uphill after momentum is lost
            if (flatVel.magnitude < minSlideSpeed && elapsedSlideTime > slideDuration)
            {
                StopSlide();
            }
        }
        
        // Cap slide speed if it exceeds the maximum
        if (flatVel.magnitude > maxSlideSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSlideSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void StopSlide()
    {
        sliding = false;
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        rb.linearDamping = originalDrag; // Restore original drag
        
        // Make sure jumping is re-enabled immediately
        readyToJump = true;
    }

    private void CheckForWall()
    {
        // Cast rays at different heights for better wall detection
        Vector3 rayStart = transform.position;
        
        // Check at player's mid-level
        bool rightWallMid = Physics.Raycast(rayStart, orientation.right, out rightWallhit, wallCheckDistance, whatIsWall);
        bool leftWallMid = Physics.Raycast(rayStart, -orientation.right, out leftWallhit, wallCheckDistance, whatIsWall);
        
        // Check slightly above
        Vector3 topRayStart = rayStart + Vector3.up * 0.5f;
        bool rightWallTop = Physics.Raycast(topRayStart, orientation.right, wallCheckDistance, whatIsWall);
        bool leftWallTop = Physics.Raycast(topRayStart, -orientation.right, wallCheckDistance, whatIsWall);
        
        // Check slightly below
        Vector3 bottomRayStart = rayStart - Vector3.up * 0.5f;
        bool rightWallBottom = Physics.Raycast(bottomRayStart, orientation.right, wallCheckDistance, whatIsWall);
        bool leftWallBottom = Physics.Raycast(bottomRayStart, -orientation.right, wallCheckDistance, whatIsWall);
        
        // Need to detect wall at least at two different heights
        wallRight = (rightWallMid && (rightWallTop || rightWallBottom));
        wallLeft = (leftWallMid && (leftWallTop || leftWallBottom));
    }

    public bool IsPlayerTouchingWall(out Vector3 hitNormal)
    {
        hitNormal = Vector3.zero;
        if (wallLeft)
        {
            hitNormal = leftWallhit.normal;
            return true;
        }
        if (wallRight)
        {
            hitNormal = rightWallhit.normal;
            return true;
        }
        return false;
    }

    private void HandleWallRidingInput()
    {
        if (isClimbing) return; // Disable wall riding while climbing

        bool wallNearby = wallLeft || wallRight;

        // If not near a wall, immediately stop wall riding to prevent getting stuck
        if (!wallNearby && wallRiding)
        {
            StopWallRide();
            return;
        }

        // Start wall ride when airborne near a wall and holding jump
        if (wallNearby && !grounded && Input.GetKey(jumpKey) && !wallRiding)
        {
            StartWallRide();
        }
        // Wall jump when player releases jump key while wall riding
        else if (wallRiding && Input.GetKeyUp(jumpKey))
        {
            WallJump();
        }
        // Also stop wall ride if jump key is not held
        else if (wallRiding && !Input.GetKey(jumpKey))
        {
            StopWallRide();
        }
    }

    private void StartWallRide()
    {
        wallRiding = true;
        readyToJump = false; // Prevent normal jump while starting wall ride
        rb.useGravity = false; // Disable gravity for full control over wall run physics
    }    private void WallRunning()
    {
        if (!wallLeft && !wallRight)
        {
            StopWallRide();
            return;
        }

        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

        if (Vector3.Dot(orientation.forward, wallForward) < 0)
            wallForward = -wallForward;

        // Directly set velocity for consistent speed and downward slide
        rb.linearVelocity = new Vector3(wallForward.x * wallRunSpeed, -wallRunSlideSpeed, wallForward.z * wallRunSpeed);

        // Apply forces to stick to the wall
        rb.AddForce(-wallNormal * wallStickForce, ForceMode.Force);
    }    private void StopWallRide()
    {        wallRiding = false;
        rb.useGravity = true;
        rb.linearDamping = groundDrag; // Reset to ground drag
        
        // Make sure jumping is re-enabled
        readyToJump = true;
        
        // Give a small outward push to prevent sticking to the wall
        if (wallLeft || wallRight)
        {
            Vector3 pushDirection = wallRight ? rightWallhit.normal : leftWallhit.normal;
            rb.AddForce(pushDirection * 3f, ForceMode.Impulse);
        }
    }

    private void WallJump()
    {
        if (!wallRiding) return;

        // Determine which wall we're jumping from
        Vector3 wallNormal = wallRight ? rightWallhit.normal : leftWallhit.normal;
        
        // Mix the camera's forward direction with the wall normal for a more intuitive jump
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Remove vertical component
        cameraForward.Normalize();
        
        // Create a jump direction that's influenced by where the camera is looking
        // but also ensures the player jumps away from the wall
        Vector3 jumpDirection = (cameraForward + wallNormal).normalized;
        
        // Stop wall riding before applying forces
        StopWallRide();
        
        // Make sure the player is pushed away from the wall
        float dotProduct = Vector3.Dot(jumpDirection, wallNormal);
        if (dotProduct < 0.25f) // If not jumping away from wall enough
        {
            jumpDirection = (jumpDirection + wallNormal).normalized; // Add more wall normal influence
        }

        // Reset velocity before applying jump forces for consistency
        rb.linearVelocity = Vector3.zero;
        
        // Apply vertical jump force
        rb.AddForce(Vector3.up * wallJumpUpForce, ForceMode.Impulse);
        
        // Apply horizontal jump force away from the wall
        rb.AddForce(jumpDirection * wallJumpSideForce, ForceMode.Impulse);

        // Allow another jump shortly after
        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void WallClimbingMovement()
    {
        Debug.Log("WallClimbingMovement executing!");
        // Ensure wallClimbSpeed has a reasonable default if not set
        float climbSpeed = wallClimbSpeed > 0 ? wallClimbSpeed : 5f;
        
        // Set vertical velocity to climb upward, reduce horizontal movement
        rb.linearVelocity = new Vector3(0f, climbSpeed, 0f);
        Debug.Log($"Set velocity to: (0, {climbSpeed}, 0)");
        
        // Push slightly into the wall to stay attached
        if (wallClimbNormal != Vector3.zero)
        {
            rb.AddForce(-wallClimbNormal * wallStickForce * 0.3f, ForceMode.Force);
            Debug.Log($"Applying wall stick force: {-wallClimbNormal * wallStickForce * 0.3f}");
        }
    }

    public void ActivateWallClimbMode(bool isActive)
    {
        wallClimbModeActive = isActive;
    }

    public void StartWallClimb()
    {
        isClimbing = true;
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero; // Stop all movement when starting climb
        
        // Lock the camera and set its pitch
        if (playerCamera != null)
        {
            playerCamera.LockCameraAndSetPitch(wallClimbCameraPitch);
        }
    }

    public void StopWallClimb(bool slideDown = true)
    {
        isClimbing = false;
        rb.useGravity = true;

        // Unlock the camera
        if (playerCamera != null)
        {
            playerCamera.UnlockCamera();
        }

        if (slideDown)
        {
            // Briefly slide down the wall
            rb.AddForce(-wallClimbNormal * 5f, ForceMode.Impulse);
        }
    }

    public void WallClimbJump()
    {
        if (!isClimbing) return;

        Debug.Log("Executing WallClimbJump - Attempting push away from wall normal.");
        Vector3 jumpDirection = wallClimbNormal; // The direction directly away from the wall

        StopWallClimb(false); // Stop climbing, don't slide down

        // Reset velocity to ensure a clean application of force
        rb.linearVelocity = Vector3.zero;

        // Apply a strong force directly away from the wall and a small upward force
        rb.AddForce((jumpDirection * 15f + Vector3.up * 4f), ForceMode.Impulse);
        Debug.Log($"Wall Climb Jump: Pushing with wall normal: {jumpDirection}");
    }

    private IEnumerator SmoothRotate180()
    {
        Debug.Log("SmoothRotate180 coroutine started!");
        float rotationTime = 0.3f;
        float elapsedTime = 0f;
        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        Debug.Log($"Start rotation: {startRotation.eulerAngles}, End rotation: {endRotation.eulerAngles}");

        while (elapsedTime < rotationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationTime;
            t = t * t * (3f - 2f * t);
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, t);
            yield return null;
        }


        transform.rotation = endRotation;
        Debug.Log("SmoothRotate180 completed!");
    }

    private IEnumerator SlideDownWall()
    {
        float slideTime = 0.5f; // Duration of the slide
        float elapsedTime = 0;
        while (elapsedTime < slideTime)
        {
            rb.linearVelocity = new Vector3(0, -5f, 0); // Use a public variable for slide speed
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    private void ResetPlayerState()
    {
        // Stop any special movement states
        if (sliding)
            StopSlide();
            
        if (wallRiding)
            StopWallRide();
        
        if (isClimbing)
            StopWallClimb(false);

          // Reset all movement variables
        readyToJump = true;
        wallRiding = false;
        sliding = false;
        wallLeft = false;
        wallRight = false;
        
        // Reset rigidbody properties
        rb.useGravity = true;
        rb.linearDamping = groundDrag;
        rb.linearVelocity = Vector3.zero;
        
        // Reset move speed
        moveSpeed = origionalMoveSpeed;
        
        // Ensure normal scale
        transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
          // Reset camera tilt immediately
        if (cameraTransform != null)
        {
            currentCameraTilt = 0f;
            targetCameraTilt = 0f;
            Vector3 currentEuler = cameraTransform.localRotation.eulerAngles;
            cameraTransform.localRotation = Quaternion.Euler(currentEuler.x, currentEuler.y, 0f);
        }
          Debug.Log("Player state reset!");
    }
      private void UpdateCameraTilt()
    {
        // --- TEMPORARY: Disable this entire function to test camera lock ---
        if (isClimbing) return;

        if (cameraTransform == null) return;

        // Set target tilt based on wall riding or climbing state
        if (wallRiding)
        {
            targetCameraTilt = wallLeft ? -wallRunCameraTilt : wallRunCameraTilt;
            Debug.Log($"Camera tilt for wall riding: {targetCameraTilt}");
        }
        else
        {
            targetCameraTilt = 0f;
        }

        // Smoothly interpolate current tilt towards target
        currentCameraTilt = Mathf.Lerp(currentCameraTilt, targetCameraTilt, Time.deltaTime * 6f);

        // Apply tilt to camera while preserving mouse look rotation
        Vector3 euler = cameraTransform.localRotation.eulerAngles;
        cameraTransform.localRotation = Quaternion.Euler(euler.x, euler.y, currentCameraTilt);
    }
}
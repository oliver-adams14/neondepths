using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal; // For URP decals

// Creates animated underwater caustic light effects with automatic performance scaling
[RequireComponent(typeof(DecalProjector))]
public class UnderwaterCaustics : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Vector2 panSpeed = new Vector2(0.01f, 0.01f);      // UV panning speed
    [SerializeField] private float rotationSpeed = 5f;                           // Degrees per second
    [SerializeField] [Range(0, 1)] private float intensityPulse = 0.2f;          // Intensity fluctuation amount
    [SerializeField] private float pulseSpeed = 1f;                              // Pulse frequency
    
    [Header("Caustic Settings")]
    [SerializeField] [Range(0, 1)] private float baseOpacity = 0.5f;             // Base caustic visibility

    [Header("Performance Optimization")]
    [SerializeField] private float updateInterval = 0.033f;                      // ~30fps animation updates
    [SerializeField] private float maxDistance = 50f;                            // Culling distance
    [SerializeField] private bool useOcclusionCulling = true;                    // Camera frustum check
    
    // Component references
    private DecalProjector decalProjector;
    private Material decalMaterialInstance;
    private Camera mainCamera;
    
    // Animation state
    private Vector2 currentOffset;
    private float currentRotation;
    private float currentIntensity = 1f;
    private float baseIntensity = 1f;
    private float timeSinceLastUpdate = 0f;
    
    // Material property IDs (cached for performance)
    private int offsetPropertyID;
    private int rotationPropertyID;
    private int intensityPropertyID;
    
    private void Awake()
    {
        decalProjector = GetComponent<DecalProjector>();
        mainCamera = Camera.main;
        
        // Cache material and base values
        if (decalProjector.material != null)
        {
            // Create an instance of the material to avoid modifying the shared asset
            decalMaterialInstance = Instantiate(decalProjector.material);
            decalProjector.material = decalMaterialInstance;

            offsetPropertyID = Shader.PropertyToID("_UVOffset");
            rotationPropertyID = Shader.PropertyToID("_Rotation");
            intensityPropertyID = Shader.PropertyToID("_Intensity");
            
            // Store initial values from the material instance
            if (decalMaterialInstance.HasProperty(intensityPropertyID))
            {
                baseIntensity = decalMaterialInstance.GetFloat(intensityPropertyID);
            }
            currentIntensity = baseIntensity;
        }
    }
    
    private void OnDestroy()
    {
        // Clean up the created material instance
        if (decalMaterialInstance != null)
        {
            Destroy(decalMaterialInstance);
        }
    }

    private void Update()
    {
        // Performance check - don't update if too far from camera
        if (!ShouldUpdate())
        {
            return;
        }
        
        // Throttle updates for performance
        timeSinceLastUpdate += Time.deltaTime;
        if (timeSinceLastUpdate < updateInterval) 
        {
            return;
        }
        timeSinceLastUpdate = 0f;
        
        // Update animation
        UpdateCausticsAnimation();
    }
    // Determines if this caustic effect should update this frame based on performance settings
    private bool ShouldUpdate()
    {
        // Skip updates if material is missing
        if (decalMaterialInstance == null) return false;
        
        // Find camera if it's not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return true; // No camera to check against
        }
        
        // Apply distance-based culling and LOD
        if (maxDistance > 0)
        {
            float distanceToCamera = Vector3.Distance(transform.position, mainCamera.transform.position);
            
            // Disable completely if beyond max distance
            if (distanceToCamera > maxDistance)
            {
                if (decalProjector.enabled)
                {
                    decalProjector.enabled = false;
                }
                return false;
            }
            else
            {
                // Within range - ensure enabled
                if (!decalProjector.enabled)
                {
                    decalProjector.enabled = true;
                }
                
                // Dynamic update rate based on distance (further = less frequent updates)
                float distanceRatio = distanceToCamera / maxDistance;
                float scaledInterval = updateInterval * (1 + distanceRatio * 2);
                
                if (Time.time - (timeSinceLastUpdate + Time.deltaTime) < scaledInterval)
                {
                    return false;
                }
            }
        }
        
        // Skip updates if not visible to camera
        if (useOcclusionCulling && mainCamera != null)
        {
            Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
            
            // Create bounds representing the decal projector area
            Vector3 size = decalProjector.size;
            Bounds bounds = new Bounds(transform.position, size); 
            
            if (!GeometryUtility.TestPlanesAABB(planes, bounds))
            {
                // Not visible - disable completely for better performance
                if (decalProjector.enabled)
                {
                    decalProjector.enabled = false;
                }
                return false;
            }
            else if (!decalProjector.enabled)
            {
                // Visible again - re-enable
                decalProjector.enabled = true;
            }
        }
        
        return true;
    }
    
    private void UpdateCausticsAnimation()
    {
        // Update UV offset for panning effect
        currentOffset += panSpeed * updateInterval;
        
        // Keep values in 0-1 range to avoid floating point precision issues
        if (currentOffset.x > 1f) currentOffset.x -= 1f;
        if (currentOffset.y > 1f) currentOffset.y -= 1f;
        
        // Update rotation
        currentRotation += rotationSpeed * updateInterval;
        if (currentRotation > 360f) currentRotation -= 360f;
        
        // Update intensity with pulsing effect
        if (intensityPulse > 0)
        {
            float pulseValue = Mathf.Sin(Time.time * pulseSpeed) * intensityPulse;
            currentIntensity = baseIntensity * (1f + pulseValue);
        }
        
        // Also apply pulsing to the decal projector's fade factor for opacity
        decalProjector.fadeFactor = baseOpacity;

        // Apply all values to material
        ApplyToMaterial();
    }
    
    private void ApplyToMaterial()
    {
        // Only set properties that the material actually has
        if (decalMaterialInstance.HasProperty(offsetPropertyID))
        {
            decalMaterialInstance.SetVector(offsetPropertyID, currentOffset);
        }
        
        if (decalMaterialInstance.HasProperty(rotationPropertyID))
        {
            decalMaterialInstance.SetFloat(rotationPropertyID, currentRotation);
        }
        
        if (decalMaterialInstance.HasProperty(intensityPropertyID))
        {
            decalMaterialInstance.SetFloat(intensityPropertyID, currentIntensity);
        }
    }
}

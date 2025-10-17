using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Manages underwater effects including fog, audio, and post-processing transitions
// Handles both automatic detection based on water surface height and trigger-based activation
public class UnderwaterManager : MonoBehaviour
{
    [Header("Underwater Settings")]
    [SerializeField] private Color waterColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
    [SerializeField] private float waterDensity = 0.1f;
    [SerializeField] [Range(0, 1)] private float underwaterFogDensity = 0.05f; // Lower values = clearer water
    [SerializeField] private Color underwaterFogColor = new Color(0, 0.3f, 0.5f, 0.5f);
    [SerializeField] private float fogAttenuationDistance = 15f;
    [SerializeField] private float underwaterAudioVolume = 0.5f;
    
    [Header("Performance Settings")]
    [SerializeField] private bool disableDebugLogging = true;
    [SerializeField] private bool optimizeDecals = true;
    
    [Header("Runtime Settings")]
    [SerializeField] private bool applyAtStart = true;
    [SerializeField] private bool applyWithTrigger = false;
    [SerializeField] private string playerTag = "Player";

    [Header("References")]
    [SerializeField] private AudioSource underwaterAudioSource;
    [SerializeField] private AudioSource surfaceAudioSource;
    [SerializeField] private Transform waterSurfaceTransform;
    [SerializeField] private LayerMask waterLayer;

    private Volume sceneVolume;
    private bool isUnderwater = false;
    private Camera mainCamera;
    private float defaultFogDensity;
    private Color defaultFogColor;
    private bool fogSettingsInitialized = false;

#if UNITY_EDITOR
    [MenuItem("Tools/Setup Underwater Effects")]
    public static void SetupUnderwaterEffects()
    {
        // Find existing manager or create a new one
        UnderwaterManager existingManager = Object.FindFirstObjectByType<UnderwaterManager>();
        if (existingManager != null)
        {
            Selection.activeGameObject = existingManager.gameObject;
            Debug.Log("Selected existing Underwater Manager.");
            return;
        }
        
        // Create a new GameObject for the manager
        GameObject managerObj = new GameObject("Underwater Manager");
        UnderwaterManager manager = managerObj.AddComponent<UnderwaterManager>();
        
        // Create a global volume if one doesn't exist
        Volume existingVolume = Object.FindFirstObjectByType<Volume>();
        if (existingVolume != null && existingVolume.isGlobal)
        {
            manager.sceneVolume = existingVolume;
            Debug.Log("Using existing global Volume for underwater effects.");
        }
    }
#endif
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Find the global volume in the scene
        sceneVolume = Object.FindFirstObjectByType<Volume>();
        
        if (sceneVolume == null)
        {
            Debug.LogWarning("No Volume found in scene. Only basic fog effects will be available.");
        }

        // Store the default fog settings
        defaultFogDensity = RenderSettings.fogDensity;
        defaultFogColor = RenderSettings.fogColor;
        fogSettingsInitialized = true;
        
        if (applyAtStart)
        {
            // Apply performance optimizations
            OptimizePerformance();
        }

    }

    private void Update()
    {
        if (mainCamera == null || waterSurfaceTransform == null)
            return;

        // Check if the camera is below the water surface
        bool isNowUnderwater = mainCamera.transform.position.y < waterSurfaceTransform.position.y;
        
        // If underwater state has changed
        if (isNowUnderwater != isUnderwater)
        {
            isUnderwater = isNowUnderwater;
            
            if (isUnderwater)
            {
                EnterWater();
            }
            else
            {
                ExitWater();
            }
        }
    }
    
    // Applies performance optimizations to ensure stable framerate underwater
    private void OptimizePerformance()
    {
        // Disable debug logging in builds for performance
        if (disableDebugLogging)
        {
            Debug.unityLogger.logEnabled = false;
        }
        
        // Optimize decals to reduce GPU workload
        if (optimizeDecals)
        {
            var decals = Object.FindObjectsByType<DecalProjector>(FindObjectsSortMode.None);
            foreach (var decal in decals)
            {
                // Lower draw distance and disable fade for better performance
                decal.drawDistance = Mathf.Min(decal.drawDistance, 20f);
                decal.fadeFactor = 1f;
                decal.startAngleFade = 180f;
            }
        }
    }

    private void EnterWater()
    {
        // Play splash sound
        if (surfaceAudioSource != null)
        {
            surfaceAudioSource.Play();
        }
        
        // Start underwater audio
        if (underwaterAudioSource != null)
        {
            underwaterAudioSource.volume = underwaterAudioVolume;
            underwaterAudioSource.Play();
        }
        
        // Apply fog effects
        ApplyUnderwaterFogSettings();
    }

    private void ExitWater()
    {
        // Play exit splash
        if (surfaceAudioSource != null)
        {
            surfaceAudioSource.Play();
        }
        
        // Stop underwater audio
        if (underwaterAudioSource != null)
        {
            underwaterAudioSource.Stop();
        }
        
        // Restore default fog settings
        RestoreDefaultFogSettings();
    }

    private void ApplyUnderwaterFogSettings()
    {
        if (!fogSettingsInitialized)
            return;
            
        // Enable fog using RenderSettings (works in all render pipelines)
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = underwaterFogDensity;
        RenderSettings.fogColor = underwaterFogColor;
    }

    private void RestoreDefaultFogSettings()
    {
        if (!fogSettingsInitialized)
            return;
            
        // Restore default fog settings
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = defaultFogDensity;
        RenderSettings.fogColor = defaultFogColor;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!applyWithTrigger) return;
        
        if (other.CompareTag(playerTag))
        {
            EnterWater();
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!applyWithTrigger) return;
        
        if (other.CompareTag(playerTag))
        {
            ExitWater();
        }
    }
    
    private void OnValidate()
    {
        // Update fog settings when values are changed in the inspector
        if (isUnderwater && fogSettingsInitialized)
        {
            ApplyUnderwaterFogSettings();
        }
    }
}

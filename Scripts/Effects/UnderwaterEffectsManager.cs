using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

// Lightweight underwater effects manager optimized for mobile platforms
// Handles fog and audio transitions when player enters/exits water
public class UnderwaterEffectsManager : MonoBehaviour
{
    [Header("Underwater Settings")]
    [SerializeField] private Color waterColor = new Color(0.2f, 0.5f, 0.8f, 0.5f);
    [SerializeField] private float waterDensity = 0.1f;
    [SerializeField] [Range(0, 1)] private float underwaterFogDensity = 0.05f;
    [SerializeField] private Color underwaterFogColor = new Color(0, 0.3f, 0.5f, 0.5f);
    [SerializeField] private float fogAttenuationDistance = 15f;
    [SerializeField] private float underwaterAudioVolume = 0.5f;
    
    [Header("Performance Settings")]
    [SerializeField] private bool disableDebugLogging = true;
    [SerializeField] private bool optimizeDecals = true;
    
    [Header("Runtime Settings")]
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
    
    private void Start()
    {
        mainCamera = Camera.main;
        
        // Find the global volume in the scene (for potential post-processing)
        sceneVolume = Object.FindFirstObjectByType<Volume>();
        
        // Initialize fog settings
        RenderSettings.fogDensity = underwaterFogDensity;
        RenderSettings.fogColor = underwaterFogColor;
        
        defaultFogDensity = RenderSettings.fogDensity;
        defaultFogColor = RenderSettings.fogColor;
        fogSettingsInitialized = true;
    }

    private void Update()
    {
        if (mainCamera == null || waterSurfaceTransform == null)
            return;

        // Detect water entry/exit based on camera position relative to water surface
        bool isNowUnderwater = mainCamera.transform.position.y < waterSurfaceTransform.position.y;
        
        // Apply effects only when state changes
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

    // Apply underwater effects when player enters water
    private void EnterWater()
    {
        // Play splash sound effect
        if (surfaceAudioSource != null)
        {
            surfaceAudioSource.Play();
        }
        
        // Start continuous underwater ambient audio
        if (underwaterAudioSource != null)
        {
            underwaterAudioSource.volume = underwaterAudioVolume;
            underwaterAudioSource.Play();
        }
        
        // Apply underwater fog visual effect
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
            
        // Enable fog
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = this.underwaterFogDensity;
        RenderSettings.fogColor = this.underwaterFogColor;
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
}
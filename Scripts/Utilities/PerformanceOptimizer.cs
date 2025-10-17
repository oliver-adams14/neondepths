using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Runtime performance optimization for distant objects
public class PerformanceOptimizer : MonoBehaviour
{
    [Header("Optimization Settings")]
    [SerializeField] private bool disableLogs = false;
    [SerializeField] private bool optimizeDecals = true;
    [SerializeField] private bool optimizeLights = true;
    [SerializeField] private float distanceThreshold = 50f;

    [Header("Categories to Disable")]
    [SerializeField] private List<string> logCategoriesToDisable = new List<string>();

    private List<string> disabledLoggers = new List<string>();
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (disableLogs)
        {
            DisableLogs();
        }
    }

    private void Start()
    {
        OptimizeRendering();
    }

    private void DisableLogs()
    {
        // Track categories we'd like to disable
        foreach (string logger in logCategoriesToDisable)
        {
            disabledLoggers.Add(logger);
        }

        // Disable non-essential logs
        Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
    }

    private void OptimizeRendering()
    {
        if (optimizeDecals)
        {
            OptimizeDecals();
        }

        if (optimizeLights)
        {
            OptimizeLights();
        }
    }

    private void OptimizeDecals()
    {
        var decals = Object.FindObjectsByType<DecalProjector>(FindObjectsSortMode.None);
        foreach (var decal in decals)
        {
            if (Vector3.Distance(mainCamera.transform.position, decal.transform.position) > distanceThreshold)
            {
                decal.enabled = false;
            }
            else
            {
                decal.enabled = true;
            }
        }
    }

    private void OptimizeLights()
    {
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            // Skip directional lights that affect the entire scene
            if (light.type == LightType.Directional)
                continue;

            if (Vector3.Distance(mainCamera.transform.position, light.transform.position) > distanceThreshold * 1.5f)
            {
                light.enabled = false;
            }
            else
            {
                light.enabled = true;
            }
        }
    }

    private void Update()
    {
        // Periodically update optimizations based on camera position
        if (Time.frameCount % 30 == 0) // Check every 30 frames
        {
            OptimizeRendering();
        }
    }
}
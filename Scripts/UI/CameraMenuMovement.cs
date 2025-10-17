using System.Collections.Generic;
using UnityEngine;

// Camera movement script for menu scenes that transitions between predefined positions
public class CameraMenuMovement : MonoBehaviour
{
    [Header("Positions (Assign at least 3)")]
    [SerializeField] private List<Transform> cameraPositions;
    
    [Header("Transition Settings")]
    [SerializeField] private float lerpSpeed = 2f;
    
    [Header("Global Volume")]
    [SerializeField] private GameObject globalVolume;  // Post-processing volume
    [SerializeField] private GameObject globalVolume2; // Secondary post-processing volume

    [Header("UI Canvas")]
    [SerializeField] private Canvas uiCanvas;
    
    private int currentIndex = 0;
    private bool isTransitioning = false;
    private Transform targetTransform;
    
    private void Start()
    {
        if (cameraPositions != null && cameraPositions.Count > 0)
        {
            currentIndex = 0;
            transform.position = cameraPositions[currentIndex].position;
            transform.rotation = cameraPositions[currentIndex].rotation;
        }
        
        if (globalVolume)
            globalVolume.SetActive(false);

        if (!globalVolume2)
            globalVolume2.SetActive(true);
    }
    
    // Public method to cycle forward. Assign to UI button.
    public void NextCameraPosition()
    {
        if (!isTransitioning && cameraPositions != null && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex + 1) % cameraPositions.Count;
            targetTransform = cameraPositions[currentIndex];
            isTransitioning = true;
            if (uiCanvas != null)
                uiCanvas.enabled = false;
        }
    }
    
    // Public method to cycle backward. Assign to UI button.
    public void PreviousCameraPosition()
    {
        if (!isTransitioning && cameraPositions != null && cameraPositions.Count > 0)
        {
            currentIndex = (currentIndex - 1 + cameraPositions.Count) % cameraPositions.Count;
            targetTransform = cameraPositions[currentIndex];
            isTransitioning = true;
            if (uiCanvas != null)
                uiCanvas.enabled = false;
        }
    }
    
private void Update()
{
    if (isTransitioning)
    {
        // Lerp position and rotation towards target
        transform.position = Vector3.Lerp(transform.position, targetTransform.position, Time.deltaTime * lerpSpeed);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetTransform.rotation, Time.deltaTime * lerpSpeed);
        
        // When close enough, simply disable transitioning without snapping
        if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f)
        {
            isTransitioning = false;
            if (uiCanvas != null)
                uiCanvas.enabled = true;
        }
    }
}
    
    // When camera touches an object with tag "Water", set globalVolume active.
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called with: " + other.gameObject.name);
        if (other.CompareTag("Water") && globalVolume)
        {
            Debug.Log("Water trigger entered - activating global volume");
            globalVolume.SetActive(true);
            globalVolume2.SetActive(false);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OnTriggerExit called with: " + other.gameObject.name);
        if (other.CompareTag("Water") && globalVolume)
        {
            Debug.Log("Water trigger exited - deactivating global volume");
            globalVolume.SetActive(false);
            globalVolume2.SetActive(true);
        }
    }
}
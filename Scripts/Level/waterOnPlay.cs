using UnityEngine;

// Applies water material to object renderer at runtime
public class WaterOnPlay : MonoBehaviour
{
    [SerializeField] private Material waterMaterial;
    
    private void Start()
    {
        // Apply water material to this object's renderer
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && waterMaterial != null)
        {
            renderer.material = waterMaterial;
        }
    }
}

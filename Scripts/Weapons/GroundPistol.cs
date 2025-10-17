using System;
using UnityEngine;

// Pickup script for pistol weapon
public class GroundPistol : MonoBehaviour
{
    public static event Action pistolPicked;
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the pickup
            gameObject.transform.position = new Vector3(10, -100, 10);
            
            // Notify listeners that pistol was picked up
            pistolPicked?.Invoke();
        }
    }
}

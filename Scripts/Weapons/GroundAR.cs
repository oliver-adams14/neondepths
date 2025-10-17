using System;
using UnityEngine;

// Pickup script for the AR weapon
public class GroundAR : MonoBehaviour
{
    public static event Action ARPicked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the pickup
            gameObject.transform.position = new Vector3(10, -100, 10);
            
            // Notify listeners that AR was picked up
            ARPicked?.Invoke();
        }
    }
}

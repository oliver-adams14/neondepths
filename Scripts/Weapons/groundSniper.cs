using System;
using UnityEngine;

// Pickup script for sniper weapon
public class GroundSniper : MonoBehaviour
{
    public static event Action SniperPicked;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Hide the pickup
            gameObject.transform.position = new Vector3(10, -100, 10);
            
            // Notify listeners that sniper was picked up
            SniperPicked?.Invoke();
        }
    }
}

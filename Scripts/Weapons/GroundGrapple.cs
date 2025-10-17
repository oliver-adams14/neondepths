using System;
using UnityEngine;

// Pickup script for the grapple weapon with level reset support
public class GroundGrapple : MonoBehaviour
{
    public static event Action grapplePicked;
    private bool canBePickedUp = true;

    private void Start()
    {
        // Subscribe to level reset events
        LevelReset.LvlReset += ResetGrapple;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canBePickedUp && other.CompareTag("Player"))
        {
            // Notify listeners that grapple was picked up
            grapplePicked?.Invoke();
            gameObject.SetActive(false);
            canBePickedUp = false;
        }
    }

    // Reset pickup state when level restarts
    private void ResetGrapple()
    {
        gameObject.SetActive(true);
        canBePickedUp = true;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        LevelReset.LvlReset -= ResetGrapple;
    }
}

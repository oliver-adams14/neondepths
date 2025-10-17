using System;
using UnityEngine;

// Pickup script for shotgun weapon with level reset support
public class GroundShotgun : MonoBehaviour
{
    public static event Action shotgunPicked;

    private void Start()
    {
        // Subscribe to level reset events
        LevelReset.LvlReset += ResetShotgun;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Notify listeners that shotgun was picked up
            shotgunPicked?.Invoke();
            gameObject.SetActive(false);
        }
    }

    // Reset pickup state when level restarts
    private void ResetShotgun()
    {
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        LevelReset.LvlReset -= ResetShotgun;
    }
}

using System;
using UnityEngine;

// Pickup script for rocket launcher weapon with level reset support
public class GroundRocketLauncher : MonoBehaviour
{
    public static event Action rocketLauncherPicked;
    private bool canBePickedUp = true;

    private void Start()
    {
        // Subscribe to level reset events
        LevelReset.LvlReset += ResetRocketLauncher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (canBePickedUp && other.CompareTag("Player"))
        {
            // Notify listeners that rocket launcher was picked up
            rocketLauncherPicked?.Invoke();
            gameObject.SetActive(false);
            canBePickedUp = false;
        }
    }

    // Reset pickup state when level restarts
    private void ResetRocketLauncher()
    {
        gameObject.SetActive(true);
        canBePickedUp = true;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        LevelReset.LvlReset -= ResetRocketLauncher;
    }
}

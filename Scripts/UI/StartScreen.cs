using UnityEngine;

// Manages the level start screen that pauses the game until player input
public class StartScreen : MonoBehaviour
{
    [SerializeField] private Timer timer;

    private void Update()
    {
        if (gameObject.activeInHierarchy)
        { 
            // Pause game while start screen is active
            Time.timeScale = 0f;

            // Start level when player clicks/taps
            if (Input.GetButton("Fire1") && timer != null)
            {
                timer.StartTimer();
                gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}

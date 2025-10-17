using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Manages level completion timer with minute:second:decisecond format
public class Timer : MonoBehaviour
{
    [SerializeField] private TMP_Text timerDisplay;

    private bool isTimerRunning;
    private float currentTime;
    
    private void Start()
    {
        // Subscribe to game events
        LevelReset.LvlReset += ResetTime;
        Flag.LvlWon += StopTimer;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        LevelReset.LvlReset -= ResetTime;
        Flag.LvlWon -= StopTimer;
    }
    
    private void Update()
    {
        if (isTimerRunning)
        {
            // Update timer and display
            currentTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }
    
    // Format and display the current time in M:SS:T format
    private void UpdateTimerDisplay()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        int deciseconds = Mathf.FloorToInt((currentTime * 10) % 10);
        
        timerDisplay.text = string.Format("{0:0}:{1:00}:{2}", minutes, seconds, deciseconds);
    }

    // Get the current elapsed time
    public float GetTime()
    {
        return currentTime;
    }
    
    // Reset timer when level is restarted
    private void ResetTime()
    {
        ResetTimer();
    }
    
    // Start the timer (called by start screen)
    public void StartTimer()
    {
        isTimerRunning = true;
    }

    // Stop the timer (called when level is completed)
    public void StopTimer()
    {
        isTimerRunning = false;
    }
    
    // Reset timer to zero
    public void ResetTimer()
    {
        currentTime = 0f;
        timerDisplay.text = "0:00:0";
        isTimerRunning = false;
    }
}

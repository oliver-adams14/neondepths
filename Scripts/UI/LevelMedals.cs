using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Manages level completion medals based on player's completion time
public class LevelMedals : MonoBehaviour
{
    [SerializeField] private Timer timer;
    [SerializeField] private Image medalDisplay;
    [SerializeField] private TMP_Text runTimeText;
    [SerializeField] private TMP_Text bestTimeText;

    [Header("Medal Thresholds and Visuals")]
    [SerializeField] private float bronzeTime;
    [SerializeField] private Sprite bronzeSprite;
    
    [SerializeField] private float silverTime;
    [SerializeField] private Sprite silverSprite;
    
    [SerializeField] private float goldTime;
    [SerializeField] private Sprite goldSprite;
    
    [SerializeField] private float diamondTime;
    [SerializeField] private Sprite diamondSprite;
    
    [SerializeField] private float redTime;
    [SerializeField] private Sprite redSprite;

    [Header("Leaderboard")]
    [SerializeField] private bool submitToLeaderboard = true;
    [SerializeField] private Button showLeaderboardButton;
    [SerializeField] private GameObject leaderboardPanel;
    [SerializeField] private float leaderboardDelay = 1.0f;
    [SerializeField] private bool showLeaderboardAutomatically = true;

    // Status tracking
    private bool isProcessing = false;
    private bool isSubscribed = false;

    private void Start()
    {
        // Subscribe to level completion event
        SafeSubscribe();

        // Find medal display if not assigned
        if (medalDisplay == null)
        {
            medalDisplay = GameObject.Find("Medal")?.GetComponent<Image>();
        }
        
        // Setup leaderboard button
        if (showLeaderboardButton != null)
        {
            showLeaderboardButton.onClick.AddListener(ShowLeaderboard);
        }
        
        // Store medal thresholds in player prefs
        SaveMedalThresholds();
    }

    // Store medal thresholds for this level in PlayerPrefs
    private void SaveMedalThresholds()
    {
        int currentLevel = PlayerPrefs.GetInt("lvl", 1);
        PlayerPrefs.SetFloat($"lvl{currentLevel}RedThreshold", redTime);
        PlayerPrefs.SetFloat($"lvl{currentLevel}DiamondThreshold", diamondTime);
        PlayerPrefs.SetFloat($"lvl{currentLevel}GoldThreshold", goldTime);
        PlayerPrefs.SetFloat($"lvl{currentLevel}SilverThreshold", silverTime);
        PlayerPrefs.SetFloat($"lvl{currentLevel}BronzeThreshold", bronzeTime);
        PlayerPrefs.Save();
    }

    // Safely subscribe to the event only once
    private void SafeSubscribe()
    {
        if (!isSubscribed)
        {
            Flag.LvlWon += CalculateMedal;
            isSubscribed = true;
        }
    }

    // Safely unsubscribe from the event
    private void SafeUnsubscribe()
    {
        if (isSubscribed)
        {
            Flag.LvlWon -= CalculateMedal;
            isSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        SafeUnsubscribe();
        StopAllCoroutines();
    }

    private void OnDisable()
    {
        SafeUnsubscribe();
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        SafeSubscribe();
    }

    // Calculate and display medal based on completion time
    private void CalculateMedal()
    {
        // Prevent multiple simultaneous calculations
        if (isProcessing) return;
        isProcessing = true;
        
        try
        {
            if (timer == null)
            {
                Debug.LogError("Timer reference is null in LevelMedals");
                isProcessing = false;
                return;
            }
            
            float currentRunTime = timer.GetTime();
            
            if (runTimeText != null)
                runTimeText.text = "Time: " + FormatTime(currentRunTime);
            
            // Handle medal display and best times
            ProcessCompletionTime(currentRunTime);
            
            // Show leaderboard after delay if configured
            if (showLeaderboardAutomatically && leaderboardPanel != null)
            {
                StartCoroutine(ShowLeaderboardAfterDelay());
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in CalculateMedal: {e.Message}");
        }
        finally
        {
            isProcessing = false;
        }
    }

    // Process the completion time, update UI and save if it's a best time
    private void ProcessCompletionTime(float currentRunTime)
    {
        int currentLevel = PlayerPrefs.GetInt("lvl", 1);
        string levelTimeKey = $"lvl{currentLevel}Time";
        string levelMedalKey = $"lvl{currentLevel}Medal";
        
        // Initialize best time if not set
        if (!PlayerPrefs.HasKey(levelTimeKey))
        {
            PlayerPrefs.SetFloat(levelTimeKey, float.MaxValue);
        }
        
        // Get previous best time
        float previousBestTime = PlayerPrefs.GetFloat(levelTimeKey);
        
        // Show current best time
        if (bestTimeText != null)
        {
            if (previousBestTime < float.MaxValue)
                bestTimeText.text = $"Current Best Time: {FormatTime(previousBestTime)}";
            else
                bestTimeText.text = "Current Best Time: N/A";
        }
        
        // Update medal display
        if (medalDisplay != null)
            DetermineMedalForTime(currentRunTime);
        
        // Check if we have a new best time
        if (currentRunTime < previousBestTime)
        {
            // We have a new best time
            if (bestTimeText != null)
                bestTimeText.text = "New Best Time!";
            
            string medalName = medalDisplay != null ? medalDisplay.sprite.name : "Unknown";
            
            // Save new best time and medal
            PlayerPrefs.SetFloat(levelTimeKey, currentRunTime);
            PlayerPrefs.SetString(levelMedalKey, medalName);
            PlayerPrefs.Save();
            
            // Submit to leaderboard
            SubmitToLeaderboard(currentLevel, currentRunTime);
        }
        else
        {
            // Not a new best time, restore best time medal
            string bestTimeMedalName = PlayerPrefs.GetString(levelMedalKey, "");
            if (!string.IsNullOrEmpty(bestTimeMedalName) && medalDisplay != null)
            {
                RestoreBestTimeMedal(bestTimeMedalName);
            }
        }
    }

    // Submit score to leaderboard if enabled
    private void SubmitToLeaderboard(int level, float time)
    {
        if (submitToLeaderboard && LeaderboardManager.Instance != null)
        {
            _ = LeaderboardManager.Instance.SubmitScoreAsync(level, time);
        }
    }

    private static string FormatTime(double timeInSeconds)
    {
        return $"{timeInSeconds:F3}s";
    }

    // Wait before showing leaderboard to ensure submission completes
    private IEnumerator ShowLeaderboardAfterDelay()
    {
        yield return new WaitForSeconds(leaderboardDelay);
        
        // Check if component still exists
        if (this == null || !gameObject || !gameObject.activeInHierarchy)
        {
            yield break;
        }
        
        ShowLeaderboard();
    }

    // Determine medal based on completion time
    private void DetermineMedalForTime(float time)
    {
        if (time < redTime)
        {
            medalDisplay.sprite = redSprite;
        }
        else if (time < diamondTime)
        {
            medalDisplay.sprite = diamondSprite;
        }
        else if (time < goldTime)
        {
            medalDisplay.sprite = goldSprite;
        }
        else if (time < silverTime)
        {
            medalDisplay.sprite = silverSprite;
        }
        else if (time < bronzeTime)
        {
            medalDisplay.sprite = bronzeSprite;
        }
    }

    // Helper method to get saved time for any level
    public static float GetLevelBestTime(int level)
    {
        string key = $"lvl{level}Time";
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : 100000f;
    }
    
    // Helper method to get saved medal for any level
    public static string GetLevelMedal(int level)
    {
        string key = $"lvl{level}Medal";
        return PlayerPrefs.HasKey(key) ? PlayerPrefs.GetString(key) : "";
    }
    
    // Show the leaderboard UI
    private void ShowLeaderboard()
    {
        if (leaderboardPanel != null)
        {
            leaderboardPanel.SetActive(true);
        }
    }
    
    // Helper method to restore medal display for best time
    private void RestoreBestTimeMedal(string medalName)
    {
        if (medalName.Contains("Red") && redSprite != null)
            medalDisplay.sprite = redSprite;
        else if (medalName.Contains("Diamond") && diamondSprite != null)
            medalDisplay.sprite = diamondSprite;
        else if (medalName.Contains("Gold") && goldSprite != null)
            medalDisplay.sprite = goldSprite;
        else if (medalName.Contains("Silver") && silverSprite != null)
            medalDisplay.sprite = silverSprite;
        else if (medalName.Contains("Bronze") && bronzeSprite != null)
            medalDisplay.sprite = bronzeSprite;
    }
}
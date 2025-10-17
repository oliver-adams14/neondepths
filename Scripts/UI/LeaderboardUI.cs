using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;

// Manages the UI display of game leaderboards
public class LeaderboardUI : MonoBehaviour
{
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject leaderboardEntryPrefab;
    [SerializeField] private TMP_Text leaderboardTitle;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button previousLevelButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private TMP_Text currentLevelText;
    
    [Header("Refresh Settings")]
    [SerializeField] private float autoRefreshInterval = 10f; // Refresh every 10 seconds
    [SerializeField] private bool enableAutoRefresh = true;
    [SerializeField] private GameObject refreshingIndicator; // Optional UI element to show refresh status
    
    private int currentLevel = 1;
    private bool isRefreshing = false;
    private float lastRefreshTime = 0f;

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(() => gameObject.SetActive(false));
            
        if (refreshButton != null)
            refreshButton.onClick.AddListener(RefreshLeaderboard);
            
        if (previousLevelButton != null)
        {
            previousLevelButton.onClick.AddListener(() => {
                if (currentLevel > 1) {
                    currentLevel--;
                    UpdateLevelText();
                    RefreshLeaderboard();
                }
            });
        }
        
        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(() => {
                currentLevel++;
                UpdateLevelText();
                RefreshLeaderboard();
            });
        }
    }

    private async void OnEnable()
    {
        currentLevel = PlayerPrefs.GetInt("lvl", 1);
        UpdateLevelText();
        await LeaderboardManager.Instance.Initialize();
        await RefreshLeaderboardAsync();
    }

    // Update is called once per frame
    private void Update()
    {
        // Auto refresh logic if enabled
        if (enableAutoRefresh && !isRefreshing && gameObject.activeInHierarchy)
        {
            if (Time.realtimeSinceStartup - lastRefreshTime > autoRefreshInterval)
            {
                RefreshLeaderboard();
            }
        }
    }
    
    private void RefreshLeaderboard()
    {
        _ = RefreshLeaderboardAsync();
    }

    private async Task RefreshLeaderboardAsync()
    {
        if (isRefreshing) return;
        
        isRefreshing = true;
        lastRefreshTime = Time.realtimeSinceStartup;
        
        if (refreshingIndicator != null)
            refreshingIndicator.SetActive(true);
            
        ClearAllEntries();

        // Ensure the content panel has a ContentSizeFitter to handle dynamic sizing
        if (leaderboardContent != null)
        {
            ContentSizeFitter fitter = leaderboardContent.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = leaderboardContent.gameObject.AddComponent<ContentSizeFitter>();
            }
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        if (leaderboardTitle != null)
            leaderboardTitle.text = $"Loading Level {currentLevel} Leaderboard...";

        var scores = await LeaderboardManager.Instance.GetScoresAsync(currentLevel);
        
        DisplayLeaderboard(scores);

        isRefreshing = false;
        if (refreshingIndicator != null)
            refreshingIndicator.SetActive(false);
    }
    
    private void DisplayLeaderboard(List<LeaderboardEntry> entries)
    {
            if (leaderboardTitle != null)
                leaderboardTitle.text = $"Level {currentLevel} - Top Times";
            
            ClearAllEntries();
            
            if (entries == null || entries.Count == 0)
            {
                CreateEmptyMessage();
                return;
            }

            foreach (var entry in entries)
            {
                CreateLeaderboardEntry(entry);
            }
            
            Canvas.ForceUpdateCanvases();
            if (leaderboardContent != null)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(leaderboardContent as RectTransform);
            }
    }
    
    // Clear all existing entries
    private void ClearAllEntries()
    {
        if (leaderboardContent == null) return;
        
        foreach (Transform child in leaderboardContent)
        {
            Destroy(child.gameObject);
        }
    }
    
    // Create a single entry in the leaderboard
    private void CreateLeaderboardEntry(LeaderboardEntry entry)
    {
        if (leaderboardEntryPrefab == null || leaderboardContent == null) return;

        GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        entryObject.name = $"Entry_{entry.Rank}_{entry.PlayerName}";

        // Get all TMP_Text components in the prefab instance
        TMP_Text[] texts = entryObject.GetComponentsInChildren<TMP_Text>();

        // Assuming a specific order: 1st is Rank, 2nd is Name, 3rd is Time.
        // This is fragile but a common setup. A more robust solution would use unique tags or component scripts on the text fields.
        if (texts.Length >= 3)
        {
            TMP_Text rankText = texts[0];
            TMP_Text nameText = texts[1];
            TMP_Text timeText = texts[2];

            rankText.text = $"#{entry.Rank}";
            nameText.text = entry.PlayerName;
            timeText.text = FormatTime(entry.Score);

            // Highlight player's entry by changing the text color
            if (AuthenticationService.Instance.IsSignedIn && entry.PlayerId == AuthenticationService.Instance.PlayerId)
            {
                rankText.color = Color.green;
                nameText.color = Color.green;
                timeText.color = Color.green;
            }
        }
        else
        {
            Debug.LogError("Leaderboard Entry Prefab does not have the expected number of TMP_Text components.", entryObject);
        }
    }
    
    // Show message when no entries exist
    private void CreateEmptyMessage()
    {
        if (leaderboardEntryPrefab == null || leaderboardContent == null) return;
        
        GameObject entryObject = Instantiate(leaderboardEntryPrefab, leaderboardContent);
        entryObject.name = "EmptyMessage";
        
        TMP_Text nameText = entryObject.transform.Find("NameText")?.GetComponent<TMP_Text>();
        if (nameText != null)
        {
            nameText.text = "No scores recorded for this level yet.";
            // Hide other fields
            entryObject.transform.Find("RankText")?.gameObject.SetActive(false);
            entryObject.transform.Find("TimeText")?.gameObject.SetActive(false);
        }
    }
    
    private static string FormatTime(double timeInSeconds)
    {
        return $"{timeInSeconds:F3}s";
    }

    private void UpdateLevelText()
    {
        if (currentLevelText != null)
            currentLevelText.text = $"Level {currentLevel}";
    }

    // Called after level completion - enhanced for better refresh
    public void ForceRefreshAfterLevelCompletion(int level)
    {
        currentLevel = level;
        UpdateLevelText();
        gameObject.SetActive(true);
        RefreshLeaderboard();
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Authentication;

// Manages display of individual entries in the leaderboard UI
public class LeaderboardEntryDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Image medalIcon;
    
    // Optional medal sprites
    [SerializeField] private Sprite bronzeSprite;
    [SerializeField] private Sprite silverSprite;
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite redSprite;
    
    // Add properties to check if this is the player's entry and get the displayed time
    public bool IsPlayerEntry { get; private set; }
    public float DisplayedTime { get; private set; }
    
    // Track the current rank for sorting
    public int CurrentRank { get; private set; } = 999;

    // Add color for player highlights
    [SerializeField] private Color playerHighlightColor = new Color(0.8f, 1.0f, 0.8f, 1.0f);
    
    // Add reference to background image (if any)
    [SerializeField] private Image backgroundImage;
    
    // Add a unique identifier property for deduplication
    public string PlayerIdentifier { get; private set; }
    
    // Helper method to normalize player names
    private string NormalizePlayerName(string playerName)
    {
        if (string.IsNullOrEmpty(playerName))
            return playerName;
            
        // Remove any #xxxx suffix that Unity might add
        int hashIndex = playerName.IndexOf('#');
        if (hashIndex > 0)
        {
            return playerName.Substring(0, hashIndex).Trim();
        }
        
        return playerName;
    }
    
    public void Initialize(string rank, string playerName, float time, int level, string medalName = "")
    {
        DisplayedTime = time;
        
        // Normalize the player name
        string normalizedName = NormalizePlayerName(playerName);
        
        // Store normalized player name for deduplication
        PlayerIdentifier = normalizedName;
        
        // Try to parse the rank number (remove the # symbol)
        if (int.TryParse(rank.Replace("#", ""), out int rankNumber))
        {
            CurrentRank = rankNumber;
        }
        
        // Check if this is the player's entry (use normalized names)
        string localPlayerName = NormalizePlayerName(PlayerPrefs.GetString("PlayerName", "Player"));
        IsPlayerEntry = (normalizedName == localPlayerName);
        
        // Set text fields
        if (rankText != null) rankText.text = rank;
        if (nameText != null) nameText.text = normalizedName; // Use normalized name
        if (timeText != null) timeText.text = $"{time:F3}s"; // Show 3 decimal places for better readability
        
        // Show medal based on name or time
        ShowMedal(medalName, time, level);
        
        // Automatically highlight player's own entry
        if (IsPlayerEntry)
        {
            HighlightPlayerEntry();
        }
    }
    
    // Update an existing entry completely
    public void UpdateEntry(string rank, string playerName, float time, int level, string medalName = "")
    {
        // Update displayed time
        DisplayedTime = time;
        
        // Try to parse the rank number (remove the # symbol)
        if (int.TryParse(rank.Replace("#", ""), out int rankNumber))
        {
            CurrentRank = rankNumber;
        }
        
        // Set text fields
        if (rankText != null) rankText.text = rank;
        if (nameText != null) nameText.text = playerName;
        if (timeText != null) timeText.text = $"{time:F6}s";
        
        // Update medal
        if (!string.IsNullOrEmpty(medalName) && medalIcon != null)
        {
            SetMedalByName(medalName);
        }
        else
        {
            DetermineMedalForTime(time, level);
        }
    }
    
    // Add method to update time display
    public void UpdateTime(float newTime, string medalName = "")
    {
        DisplayedTime = newTime;
        
        // Update time text
        if (timeText != null)
        {
            timeText.text = $"{newTime:F6}s";
        }
        
        // Update medal if provided
        if (!string.IsNullOrEmpty(medalName) && medalIcon != null)
        {
            SetMedalByName(medalName);
        }
    }
    
    private void SetMedalByName(string medalName)
    {
        if (medalIcon == null) return;
        
        // Match medal name to sprite
        switch (medalName)
        {
            case string name when name.Contains("Red"):
                if (redSprite != null) medalIcon.sprite = redSprite;
                break;
            case string name when name.Contains("Diamond"):
                if (diamondSprite != null) medalIcon.sprite = diamondSprite;
                break;
            case string name when name.Contains("Gold"):
                if (goldSprite != null) medalIcon.sprite = goldSprite;
                break;
            case string name when name.Contains("Silver"):
                if (silverSprite != null) medalIcon.sprite = silverSprite;
                break;
            case string name when name.Contains("Bronze"):
                if (bronzeSprite != null) medalIcon.sprite = bronzeSprite;
                break;
            default:
                medalIcon.gameObject.SetActive(false);
                break;
        }
    }
    
    private void DetermineMedalForTime(float time, int level)
    {
        // Get level thresholds from PlayerPrefs or other source
        // This is just an example - you would need to store these thresholds per level
        float redTime = PlayerPrefs.GetFloat($"lvl{level}RedThreshold", 0);
        float diamondTime = PlayerPrefs.GetFloat($"lvl{level}DiamondThreshold", 0);
        float goldTime = PlayerPrefs.GetFloat($"lvl{level}GoldThreshold", 0);
        float silverTime = PlayerPrefs.GetFloat($"lvl{level}SilverThreshold", 0);
        float bronzeTime = PlayerPrefs.GetFloat($"lvl{level}BronzeThreshold", 0);
        
        if (medalIcon == null) return;
        
        // Set medal based on time
        if (redTime > 0 && time < redTime && redSprite != null)
            medalIcon.sprite = redSprite;
        else if (diamondTime > 0 && time < diamondTime && diamondSprite != null)
            medalIcon.sprite = diamondSprite;
        else if (goldTime > 0 && time < goldTime && goldSprite != null)
            medalIcon.sprite = goldSprite;
        else if (silverTime > 0 && time < silverTime && silverSprite != null)
            medalIcon.sprite = silverSprite;
        else if (bronzeTime > 0 && time < bronzeTime && bronzeSprite != null)
            medalIcon.sprite = bronzeSprite;
        else
            medalIcon.gameObject.SetActive(false);
    }

    // Shows the medal based on the provided medal name or time
    private void ShowMedal(string medalName, float time, int level)
    {
        if (medalIcon == null) return;
        
        // First ensure the medal is visible
        medalIcon.gameObject.SetActive(true);
        
        // Try to use medal name first
        if (!string.IsNullOrEmpty(medalName) && medalName != "none")
        {
            SetMedalByName(medalName);
        }
        else
        {
            // Fallback to determining medal by time
            DetermineMedalForTime(time, level);
        }
    }

    // Method to highlight the player's own entry - enhanced version
    public void HighlightPlayerEntry()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = playerHighlightColor;
        }
        
        // Make name and rank text stand out
        if (nameText != null)
        {
            nameText.fontStyle = FontStyles.Bold;
            nameText.color = Color.green;
        }
        
        if (rankText != null)
        {
            rankText.fontStyle = FontStyles.Bold;
        }
        
        // Mark this entry as the player's
        IsPlayerEntry = true;
    }
}

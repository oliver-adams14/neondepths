using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Displays level completion times and medals in the level selection menu
public class LevelPage : MonoBehaviour
{
    [Header("Level 1")]
    [SerializeField] private TMP_Text lvl1Time;
    [SerializeField] private Image lvl1Medal;    
    
    [Header("Level 2")]
    [SerializeField] private TMP_Text lvl2Time;
    [SerializeField] private Image lvl2Medal;
    
    [Header("Level 3")]
    [SerializeField] private TMP_Text lvl3Time;
    [SerializeField] private Image lvl3Medal;
    
    [Header("Level 4")]
    [SerializeField] private TMP_Text lvl4Time;
    [SerializeField] private Image lvl4Medal;
    
    [Header("Level 5")]
    [SerializeField] private TMP_Text lvl5Time;
    [SerializeField] private Image lvl5Medal;
    
    [Header("Level 6")]
    [SerializeField] private TMP_Text lvl6Time;
    [SerializeField] private Image lvl6Medal;
    
    [Header("Level 7")]
    [SerializeField] private TMP_Text lvl7Time;
    [SerializeField] private Image lvl7Medal;
    
    [Header("Level 8")]
    [SerializeField] private TMP_Text lvl8Time;
    [SerializeField] private Image lvl8Medal;
    
    [Header("Level 9")]
    [SerializeField] private TMP_Text lvl9Time;
    [SerializeField] private Image lvl9Medal;
    
    [Header("Level 10")]
    [SerializeField] private TMP_Text lvl10Time;
    [SerializeField] private Image lvl10Medal;

    [Header("Medal Sprites")]
    [SerializeField] private Sprite bronzeSprite;
    [SerializeField] private Sprite silverSprite;
    [SerializeField] private Sprite goldSprite;
    [SerializeField] private Sprite diamondSprite;
    [SerializeField] private Sprite redSprite;

    // Start is called before the first frame update
    void Start()
    {
        updateUI();
    }
    
    private void Awake()
    {
        updateUI();
    }

    private void updateUI()
    {
        // Level 1
        UpdateLevelUI(lvl1Time, lvl1Medal, "lvl1Time", "lvl1Medal");
        
        // Level 2
        UpdateLevelUI(lvl2Time, lvl2Medal, "lvl2Time", "lvl2Medal");
        
        // Level 3
        UpdateLevelUI(lvl3Time, lvl3Medal, "lvl3Time", "lvl3Medal");
        
        // Level 4
        UpdateLevelUI(lvl4Time, lvl4Medal, "lvl4Time", "lvl4Medal");
        
        // Level 5
        UpdateLevelUI(lvl5Time, lvl5Medal, "lvl5Time", "lvl5Medal");
        
        // Level 6
        UpdateLevelUI(lvl6Time, lvl6Medal, "lvl6Time", "lvl6Medal");
        
        // Level 7
        UpdateLevelUI(lvl7Time, lvl7Medal, "lvl7Time", "lvl7Medal");
        
        // Level 8
        UpdateLevelUI(lvl8Time, lvl8Medal, "lvl8Time", "lvl8Medal");
        
        // Level 9
        UpdateLevelUI(lvl9Time, lvl9Medal, "lvl9Time", "lvl9Medal");
        
        // Level 10
        UpdateLevelUI(lvl10Time, lvl10Medal, "lvl10Time", "lvl10Medal");
    }
    
    // Helper method to update a single level's UI
    private void UpdateLevelUI(TMP_Text timeText, Image medalImage, string timeKey, string medalKey)
    {
        if (timeText != null)
        {
            // Get the time value
            float time = PlayerPrefs.GetFloat(timeKey, 0f);
            
            // Check if it's the default placeholder value
            if (time >= 100000f || time <= 0f)
            {
                // This is a placeholder/default value, remove it from PlayerPrefs
                if (PlayerPrefs.HasKey(timeKey))
                {
                    PlayerPrefs.DeleteKey(timeKey);
                    Debug.Log($"Deleted placeholder time value for {timeKey}");
                }
                
                // Display placeholder text
                timeText.text = "--:--:--";
            }
            else
            {
                // Format and display the actual time
                timeText.text = FormatTime(time);
            }
        }
        
        if (medalImage != null)
        {
            // Check if there's a medal stored
            if (PlayerPrefs.HasKey(medalKey) && !string.IsNullOrEmpty(PlayerPrefs.GetString(medalKey)))
            {
                string medalType = PlayerPrefs.GetString(medalKey);
                medalSprite(medalType, medalImage);
                medalImage.gameObject.SetActive(true);
            }
            else
            {
                // No medal earned yet, hide the medal image
                medalImage.gameObject.SetActive(false);
                Debug.Log($"No medal for {medalKey}, hiding medal UI");
            }
        }
    }

    // Make this method more robust to handle null or empty medal names
    private void medalSprite(string medal, Image i)
    {
        if (string.IsNullOrEmpty(medal))
        {
            i.gameObject.SetActive(false);
            return;
        }
        
        // Debug the incoming medal value
        Debug.Log($"Setting medal: '{medal}'");
        
        // Check for WhiteSquare case - hide the medal
        if (medal.Contains("WhiteSquare"))
        {
            i.gameObject.SetActive(false);
            Debug.Log("WhiteSquare medal type detected - hiding medal");
            return;
        }
        
        // Make the medal visible by default
        i.gameObject.SetActive(true);
        
        // Try to match the medal name with more flexible comparison
        if (medal.Contains("Bronze"))
        {
            i.sprite = bronzeSprite;
            Debug.Log("Set Bronze sprite");
        }
        else if (medal.Contains("Silver"))
        {
            i.sprite = silverSprite;
            Debug.Log("Set Silver sprite");
        }
        else if (medal.Contains("Gold"))
        {
            i.sprite = goldSprite;
            Debug.Log("Set Gold sprite");
        }
        else if (medal.Contains("Diamonde") || medal.Contains("Blue"))
        {
            i.sprite = diamondSprite;
            Debug.Log("Set Diamond/Blue sprite");
        }
        else if (medal.Contains("Ruby"))
        {
            i.sprite = redSprite;
            Debug.Log("Set Red sprite");
        }
        else
        {
            // Unknown medal type, hide the medal
            i.gameObject.SetActive(false);
            Debug.LogWarning($"Unknown medal type: '{medal}'");
        }
    }
    // Format time as minutes:seconds.milliseconds
    private string FormatTime(float timeInSeconds)
    {
        if (timeInSeconds <= 0) return "00:00.00";
        
        int minutes = (int)(timeInSeconds / 60);
        int seconds = (int)(timeInSeconds % 60);
        int milliseconds = (int)((timeInSeconds * 100) % 100);
        
        return string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, milliseconds);
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
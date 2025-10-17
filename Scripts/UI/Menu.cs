using UnityEngine;
using UnityEngine.SceneManagement;

// Main menu controller that handles level selection and initialization
public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject namePromptPanel;

    private void Start()
    {
        // Enable cursor for menu navigation
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            // Reset level tracking when returning to main menu
            PlayerPrefs.SetInt("lvl", 0);
            PlayerPrefs.Save();
            
            // Show name prompt if player name not set
            ShowNamePromptIfNeeded();
        }
    }

    private void ShowNamePromptIfNeeded()
    {
        if (namePromptPanel != null)
        {
            bool hasName = !string.IsNullOrEmpty(PlayerPrefs.GetString("PlayerName", ""));
            namePromptPanel.SetActive(!hasName);
        }
    }

    // Level selection methods - called by UI buttons
    // Helper method to load levels with consistent tracking
    private void LoadLevel(int levelNumber, string sceneName)
    {
        PlayerPrefs.SetInt("lvl", levelNumber);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    
    // Level selection methods - called by UI buttons
    public void lvl1()
    {
        LoadLevel(1, "LEVEL_1");
    }

    public void lvl2()
    {
        LoadLevel(2, "LEVEL_2");
    }

    public void lvl3()
    {
        LoadLevel(3, "LEVEL_3");
    }

    public void lvl4()
    { 
        LoadLevel(4, "LEVEL_4");
    }

    public void lvl5()
    { 
        LoadLevel(5, "LEVEL_5");
    }

    public void lvl6()
    { 
        LoadLevel(6, "LEVEL_6");
    }

    public void lvl7()
    {  
        LoadLevel(7, "LEVEL_7");
    }

    public void lvl8()
    {
        LoadLevel(8, "LEVEL_8");
    }

    public void lvl9()
    {  
        LoadLevel(9, "LEVEL_9");
    }

    public void lvl10()
    {
        LoadLevel(10, "LEVEL_10");
    }

    // Return to main menu
    public void menu()
    {
        SceneManager.LoadScene("Menu", LoadSceneMode.Single);
    }
}
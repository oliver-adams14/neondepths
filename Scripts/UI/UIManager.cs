using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Main UI manager that handles weapon display, health, and game state screens
public class UIManager : MonoBehaviour
{
    [Header("Weapon UI")]
    [SerializeField] private Slider chargeSlider;
    [SerializeField] private GameObject pistolIcon;
    [SerializeField] private GameObject arIcon;
    [SerializeField] private GameObject sniperIcon;
    [SerializeField] private GameObject basicIcon;
    [SerializeField] private GameObject shotgunIcon;

    [Header("Ammo Display")]
    [SerializeField] private GameObject notchPrefab;    // Visual indicator for each ammo unit
    [SerializeField] private RectTransform notchContainer;
    private List<GameObject> currentNotches = new List<GameObject>();

    [Header("Weapon Colors")]
    [SerializeField] private Image chargeSliderFill;
    [SerializeField] private Color pistolColor = Color.green;
    [SerializeField] private Color arColor = Color.yellow;
    [SerializeField] private Color sniperColor = Color.blue;
    [SerializeField] private Color basicColor = Color.grey;
    [SerializeField] private Color shotgunColor = Color.red;

    [Header("Game State UI")]
    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject deadScreen;
    [SerializeField] private GameObject crosshair;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject[] hearts;      // Health indicators
    
    // Singleton pattern
    private static UIManager instance = null;
    public static UIManager Instance => instance;
    
    private void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
    }
    
    private void Start()
    {
        // Subscribe to events
        greenEnemyBullet.playerHit += loseHealth;
        LevelReset.LvlReset += ResetManager;
    }

    private void OnDestroy()
    {
        // Unsubscribe to prevent memory leaks
        if (instance == this)
        {
            greenEnemyBullet.playerHit -= loseHealth;
            LevelReset.LvlReset -= ResetManager;
        }
    }
    
    // Reset all UI elements when level restarts
    private void ResetManager()
    {
        // Lock cursor for gameplay
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        Time.timeScale = 1f;
        
        // Reset health indicators
        foreach (var heart in hearts)
        {
            if (heart != null)
            {
                heart.SetActive(true);
            }
        }
        if (crosshair != null)
        {
            crosshair.SetActive(true);
        }
        if (winScreen != null)
        {
            winScreen.SetActive(false);
        }
        if (deadScreen != null)
        {
            deadScreen.SetActive(false);
        }
        if (settingsMenu != null)
        {
            settingsMenu.SetActive(false);
        }
        
        UpdateChargeBar(100f, 100f); // Reset charge bar to full
        UpdateWeaponUI("basic"); // Reset to default weapon
        
        Debug.Log("UI Manager reset completed");
    }

    public void UpdateChargeBar(float current, float max)
    {
        if (chargeSlider != null)
        {
            chargeSlider.value = current / max;
        }
    }

    public void UpdateWeaponUI(string weaponType)
    {
        if (pistolIcon != null) pistolIcon.SetActive(weaponType == "pistol");
        if (arIcon != null) arIcon.SetActive(weaponType == "AR");
        if (sniperIcon != null) sniperIcon.SetActive(weaponType == "Sniper");
        if (basicIcon != null) basicIcon.SetActive(weaponType == "basic");
        if (shotgunIcon != null) shotgunIcon.SetActive(weaponType == "shotgun");

        // Update the charge slider fill color based on the weapon type
        if (chargeSliderFill != null)
        {
            switch (weaponType)
            {
                case "basic":
                    chargeSliderFill.color = basicColor;
                    break;
                case "pistol":
                    chargeSliderFill.color = pistolColor;
                    break;
                case "AR":
                    chargeSliderFill.color = arColor;
                    break;
                case "Sniper":
                    chargeSliderFill.color = sniperColor;
                    break;
                case "shotgun":
                    chargeSliderFill.color = shotgunColor;
                    break;
                default:
                    chargeSliderFill.color = Color.white; // Default color
                    break;
            }
        }
    }

    public void UpdateNotches(float maxCharge, float costPerShot)
    {
        // Clear any existing notches
        foreach (GameObject notch in currentNotches)
        {
            Destroy(notch);
        }
        currentNotches.Clear();

        if (costPerShot <= 0 || notchPrefab == null || notchContainer == null)
        {
            return; // Don't draw if cost is zero or prefabs aren't set
        }

        // Use the container's width, which represents the slider's maximum possible width
        float sliderWidth = notchContainer.rect.width;
        int notchCount = (int)(maxCharge / costPerShot);

        for (int i = 1; i < notchCount; i++)
        {
            GameObject notchInstance = Instantiate(notchPrefab, notchContainer);
            RectTransform notchRect = notchInstance.GetComponent<RectTransform>();

            // Calculate position based on the total width
            float xPos = (sliderWidth / notchCount) * i;
            notchRect.anchoredPosition = new Vector2(xPos, 0);
            
            currentNotches.Add(notchInstance);
        }
    }

    // Handle player taking damage and update health UI
    private void loseHealth()
    {
        bool tookDamage = false;
        bool noHeartsLeft = true;
        
        // Remove one heart (health point)
        if (hearts.Length > 0) 
        {
            for (int i = 0; i < hearts.Length; i++)
            {
                if (hearts[i].activeInHierarchy && !tookDamage)
                {
                    tookDamage = true;
                    hearts[i].SetActive(false);
                    break;
                }
            }
        }
        
        // Check if any hearts remain
        for (int i = 0; i < hearts.Length; i++)
        {
            if (hearts[i].activeInHierarchy)
            {
                noHeartsLeft = false;
            }
        }
        if (noHeartsLeft)
        {
            Time.timeScale = 0;
            deadScreen.SetActive(true);
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            if (crosshair != null)
            {
                crosshair.SetActive(false);
            }
            UnityEngine.Cursor.visible = true;
        }
    }

    // Show victory screen
    public void ShowWinScreen()
    {
        // Show cursor and hide game UI
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
        
        if (crosshair != null)
        {
            crosshair.SetActive(false);
        }
        
        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
    }

    // Hide all weapon icons
    public void ClearWeaponUI()
    {
        if (pistolIcon != null) pistolIcon.SetActive(false);
        if (arIcon != null) arIcon.SetActive(false);
        if (sniperIcon != null) sniperIcon.SetActive(false);
        if (basicIcon != null) basicIcon.SetActive(false);
        if (shotgunIcon != null) shotgunIcon.SetActive(false);
    }

}
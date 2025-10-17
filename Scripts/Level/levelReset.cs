using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

// Manages level reset functionality, restoring all game objects to their initial state
// Stores initial positions and rotations of objects and handles resetting UI, enemies, and player
public class LevelReset : MonoBehaviour
{
    // Store references to important objects
    [SerializeField] private GameObject player; // Assign in inspector instead of finding by tag
    [SerializeField] private GameObject cam;
    [SerializeField] private GameObject startScreen;
    
    // Store the initial positions and rotations of objects that need to be reset
    private Vector3 initialPlayerPosition;
    private Quaternion initialPlayerRotation;
    private Vector3 initialCamPosition;
    private Quaternion initialCamRotation;
    
    private Vector3[] initialWeaponsPositions;
    private Quaternion[] initialWeaponRotations;
    private Vector3[] rinitialEnemyPositions;
    private Quaternion[] rinitialEnemyRotations;
    private Vector3[] ginitialEnemyPositions;
    private Quaternion[] ginitialEnemyRotations;

    public static event Action LvlReset;
    
    private bool initialValuesStored = false;
    private Rigidbody playerRigidbody;
      void Start()
    {
        Debug.Log("LevelReset script started!");
        
        // Find player if not assigned
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
            
        if (player != null)
            playerRigidbody = player.GetComponent<Rigidbody>();
            
        // Store the initial positions and rotations during initialization
        StoreInitialValues();
    }

    void Update()
    {
        // Check for a reset input (e.g., pressing a key)
        if (Input.GetKeyDown(KeyCode.F)) // Changed back to F key for level reset
        {
            Debug.Log("F key pressed - attempting level reset");
            // Reset the level
            ResetLevel();
        }
    }

    void StoreInitialValues()
    {
        bool allValuesStored = true;
        
        // Store the initial position and rotation of the player
        if (player != null)
        {
            initialPlayerPosition = player.transform.position;
            initialPlayerRotation = player.transform.rotation;
            Debug.Log($"Stored initial player position: {initialPlayerPosition}");
        }
        else
        {
            allValuesStored = false;
            Debug.LogError("Player not found for storing initial position");
        }
        
        if (cam != null)
        {
            initialCamPosition = cam.transform.position;
            initialCamRotation = cam.transform.rotation;
        }
        else
        {
            allValuesStored = false;
            Debug.LogError("Camera not found for storing initial position");
        }

        // Store weapons positions
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapons");
        if (weapons != null && weapons.Length > 0)
        {
            initialWeaponsPositions = new Vector3[weapons.Length];
            initialWeaponRotations = new Quaternion[weapons.Length];
            
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null) {
                    initialWeaponsPositions[i] = weapons[i].transform.position;
                    initialWeaponRotations[i] = weapons[i].transform.rotation;
                }
            }
        }

        // Store enemy positions with more robust checks
        StoreEnemyPositions();
        
        initialValuesStored = allValuesStored;
        Debug.Log($"Initial values stored successfully: {initialValuesStored}");
    }
    
    private void StoreEnemyPositions()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies != null && enemies.Length > 0)
        {
            // Count red and green enemies first to allocate arrays properly
            int redCount = 0;
            int greenCount = 0;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy.name.Contains("Red")) redCount++;
                else if (enemy.name.Contains("Green")) greenCount++;
            }
            
            // Initialize arrays with correct sizes
            rinitialEnemyPositions = new Vector3[redCount];
            rinitialEnemyRotations = new Quaternion[redCount];
            ginitialEnemyPositions = new Vector3[greenCount];
            ginitialEnemyRotations = new Quaternion[greenCount];
            
            // Store positions
            int rIndex = 0;
            int gIndex = 0;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy.name.Contains("Red") && rIndex < redCount)
                {
                    rinitialEnemyPositions[rIndex] = enemy.transform.position;
                    rinitialEnemyRotations[rIndex] = enemy.transform.rotation;
                    rIndex++;
                }
                else if (enemy.name.Contains("Green") && gIndex < greenCount)
                {
                    ginitialEnemyPositions[gIndex] = enemy.transform.position;
                    ginitialEnemyRotations[gIndex] = enemy.transform.rotation;
                    gIndex++;
                }
            }
        }
    }    public void ResetLevel()
    {
        Debug.Log("Starting comprehensive level reset...");
        
        // Make sure we have valid initial values
        if (!initialValuesStored)
        {
            Debug.LogWarning("Cannot reset level - initial values not properly stored");
            StoreInitialValues(); // Try to store them now
            if (!initialValuesStored) return; // If still failed, abort reset
        }        // 1. Hide any active UI screens and reset UI elements
        ResetUI();

        // 2. Reset Player Position and Rotation
        ResetPlayer();
        
        // 3. Reset Player Health (if health system exists)
        ResetPlayerHealth();
        
        // 4. Reset Timer
        ResetTimer();
        
        // 5. Reset Weapons
        ResetWeapons();
        
        // 6. Reset Enemies
        ResetEnemies();
        
        // 7. Destroy all bullets and projectiles
        DestroyAllBullets();

        // 8. Reset Player Movement State
        ResetPlayerMovementState();

        // 9. Enable player controls if they were disabled
        EnablePlayerControls();        // 10. Trigger the level reset event for other systems
        Debug.Log("Broadcasting level reset event to all subscribers...");
        if (LvlReset != null)
        {
            LvlReset();
            Debug.Log("Level reset event broadcast completed");
        }
        else
        {
            Debug.LogWarning("No subscribers to level reset event!");
        }
          // 11. Show the start screen UI
        if (startScreen != null)
        {
            startScreen.SetActive(true);
            Time.timeScale = 0f; // Start screen pauses the game
            Debug.Log("Start screen activated");
        }
        else
        {
            Debug.LogError("Start screen GameObject not assigned in levelReset component!");
        }
        
        Debug.Log("Level reset completed successfully!");
    }

    // Add this new helper method to re-enable player controls
    private void EnablePlayerControls()
    {
        if (player != null)
        {
            // Enable movement script
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }
            
            // Enable weapon manager
            WeaponManager weaponManager = player.GetComponent<WeaponManager>();
            if (weaponManager != null)
            {
                weaponManager.enabled = true;
            }
            
            // Make sure the player is active
            player.SetActive(true);
            
            // Enable rigidbody if it was disabled
            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = false;
                playerRigidbody.detectCollisions = true;
            }
        }
    }
    private void DestroyAllBullets()
    {
        GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");
        GameObject[] G = GameObject.FindGameObjectsWithTag("G");
        foreach (GameObject bullet in bullets)
        {
            if (bullet != null)
                Destroy(bullet);
        }
        foreach (GameObject g in G)
        {
            if (g != null)
                Destroy(g);
        }
    }
      private void ResetPlayer()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogError("Player not found during reset");
                return;
            }
        }
        
        // Reset position and rotation
        player.transform.position = initialPlayerPosition;
        player.transform.rotation = initialPlayerRotation;
        Debug.Log($"Reset player to position: {initialPlayerPosition}");

        // Reset rigidbody if present
        if (playerRigidbody == null)
            playerRigidbody = player.GetComponent<Rigidbody>();
            
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.Sleep(); // Ensure physics engine doesn't override our position
        }
        
        // Reset camera position and rotation
        if (cam != null)
        {
            cam.transform.position = initialCamPosition;
            cam.transform.rotation = initialCamRotation;
            Debug.Log("Camera position and rotation reset");
        }
        
        // Reset any camera tilt from wall running
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.localRotation = Quaternion.Euler(mainCam.transform.localRotation.eulerAngles.x, mainCam.transform.localRotation.eulerAngles.y, 0f);
        }
    }
    
    private void ResetWeapons()
    {
        GameObject[] weapons = GameObject.FindGameObjectsWithTag("Weapons");
        if (weapons != null && initialWeaponsPositions != null && 
            weapons.Length <= initialWeaponsPositions.Length)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                if (weapons[i] != null)
                {
                    weapons[i].transform.position = initialWeaponsPositions[i];
                    weapons[i].transform.rotation = initialWeaponRotations[i];
                    weapons[i].SetActive(true);
                }
            }
        }
    }    private void ResetEnemies()
    {
        Debug.Log("Starting enemy reset...");
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Debug.Log($"Found {enemies.Length} enemies to reset");
        
        if (enemies != null)
        {
            int rCount = 0;
            int gCount = 0;
            
            foreach (GameObject enemy in enemies)
            {
                if (enemy == null) continue;
                
                Debug.Log($"Resetting enemy: {enemy.name}");
                
                // Reset position and rotation based on enemy type
                if (enemy.name.Contains("Red") && rinitialEnemyPositions != null && 
                    rCount < rinitialEnemyPositions.Length)
                {
                    enemy.transform.position = rinitialEnemyPositions[rCount];
                    enemy.transform.rotation = rinitialEnemyRotations[rCount];
                    enemy.SetActive(true);
                    Debug.Log($"Red enemy {enemy.name} reset to position: {rinitialEnemyPositions[rCount]}");
                    
                    // Reset enemy health if it has a health component
                    ResetEnemyHealth(enemy);
                    
                    rCount++;
                }
                else if (enemy.name.Contains("Green") && ginitialEnemyPositions != null && 
                        gCount < ginitialEnemyPositions.Length)
                {
                    enemy.transform.position = ginitialEnemyPositions[gCount];
                    enemy.transform.rotation = ginitialEnemyRotations[gCount];
                    enemy.SetActive(true);
                    Debug.Log($"Green enemy {enemy.name} reset to position: {ginitialEnemyPositions[gCount]}");
                    
                    // Reset enemy health if it has a health component
                    ResetEnemyHealth(enemy);
                    
                    gCount++;
                }
                
                // Reset enemy rigidbody
                Rigidbody enemyRb = enemy.GetComponent<Rigidbody>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = Vector3.zero;
                    enemyRb.angularVelocity = Vector3.zero;
                }
            }
            Debug.Log($"Reset {enemies.Length} enemies - {rCount} red, {gCount} green");
        }
        else
        {
            Debug.LogWarning("No enemies found to reset!");
        }
    }
    
      private void ResetUI()
    {
        Debug.Log("Starting UI reset...");
        
        // Reset time scale first
        Time.timeScale = 1f;
        
        // Try multiple possible names for UI elements
        string[] deadScreenNames = {"DeadScreen", "Dead Screen", "GameOverScreen", "Game Over Screen", "deadScreen"};
        string[] winScreenNames = {"WinScreen", "Win Screen", "VictoryScreen", "Victory Screen", "winScreen"};
        string[] settingsNames = {"SettingsMenu", "Settings Menu", "Settings", "PauseMenu", "Pause Menu", "settingsMenu"};
        
        // Hide dead screen
        bool deadScreenFound = false;
        foreach (string name in deadScreenNames)
        {
            GameObject deadScreen = GameObject.Find(name);
            if (deadScreen != null)
            {
                deadScreen.SetActive(false);
                deadScreenFound = true;
                Debug.Log($"Dead screen found and hidden: {name}");
                break;
            }
        }
        if (!deadScreenFound) Debug.LogWarning("Dead screen not found with any common name");
        
        // Hide win screen
        bool winScreenFound = false;
        foreach (string name in winScreenNames)
        {
            GameObject winScreen = GameObject.Find(name);
            if (winScreen != null)
            {
                winScreen.SetActive(false);
                winScreenFound = true;
                Debug.Log($"Win screen found and hidden: {name}");
                break;
            }
        }
        if (!winScreenFound) Debug.LogWarning("Win screen not found with any common name");
        
        // Hide settings menu
        bool settingsFound = false;
        foreach (string name in settingsNames)
        {
            GameObject settingsMenu = GameObject.Find(name);
            if (settingsMenu != null)
            {
                settingsMenu.SetActive(false);
                settingsFound = true;
                Debug.Log($"Settings menu found and hidden: {name}");
                break;
            }
        }
        if (!settingsFound) Debug.LogWarning("Settings menu not found with any common name");
        
        // Reset cursor state
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        
        Debug.Log("UI state reset completed");
    }private void ResetEnemyHealth(GameObject enemy)
    {
        // The Enemy script has its own ResetEnemy method that's subscribed to the lvlReset event
        // So we don't need to manually reset enemy health here - it's handled automatically
        // However, we can add some debug logging to verify enemies are being found
        
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            Debug.Log($"Found Enemy script on {enemy.name} - enemy will reset via event subscription");
        }
        else
        {
            Debug.LogWarning($"No Enemy script found on {enemy.name}");
        }
    }
    
    private void ResetPlayerHealth()
    {
        // Try to find and reset health system
        if (player != null)
        {
            // Look for common health component names
            MonoBehaviour healthComponent = player.GetComponent("Health") as MonoBehaviour;
            if (healthComponent == null)
                healthComponent = player.GetComponent("PlayerHealth") as MonoBehaviour;
            if (healthComponent == null)
                healthComponent = player.GetComponent("HealthManager") as MonoBehaviour;
                
            if (healthComponent != null)
            {
                // Try to reset health using reflection or common methods
                var healthField = healthComponent.GetType().GetField("currentHealth");
                var maxHealthField = healthComponent.GetType().GetField("maxHealth");
                
                if (healthField != null && maxHealthField != null)
                {
                    var maxHealth = maxHealthField.GetValue(healthComponent);
                    healthField.SetValue(healthComponent, maxHealth);
                    Debug.Log("Player health reset");
                }
                else
                {
                    // Try common method names
                    var resetMethod = healthComponent.GetType().GetMethod("ResetHealth");
                    if (resetMethod != null)
                    {
                        resetMethod.Invoke(healthComponent, null);
                        Debug.Log("Player health reset via method");
                    }
                }
            }
        }
    }    private void ResetTimer()
    {
        Debug.Log("Starting timer reset...");
        
        // Try multiple approaches to find and reset the timer
        timer timerComponent = null;
        
        // Method 1: Look for timer by GameObject name
        string[] timerNames = {"Timer", "GameTimer", "TimerUI", "timer", "TimerText"};
        foreach (string name in timerNames)
        {
            GameObject timerObject = GameObject.Find(name);
            if (timerObject != null)
            {
                timerComponent = timerObject.GetComponent<timer>();
                if (timerComponent != null)
                {
                    Debug.Log($"Timer found by name: {name}");
                    break;
                }
            }
        }
        
        // Method 2: Find timer component directly in scene
        if (timerComponent == null)
        {
            timerComponent = FindFirstObjectByType<timer>();
            if (timerComponent != null)
            {
                Debug.Log("Timer found via FindFirstObjectByType");
            }
        }
        
        // Reset the timer directly
        if (timerComponent != null)
        {
            timerComponent.resetTimer();
            Debug.Log("Timer reset directly - will start via start screen");
        }
        else
        {
            Debug.LogError("No timer component found in scene - timer will not reset!");
        }
    }
    
    private void ResetPlayerMovementState()
    {
        if (player != null)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Use the existing reset method if available
                var resetMethod = playerMovement.GetType().GetMethod("ResetPlayerState");
                if (resetMethod != null)
                {
                    resetMethod.Invoke(playerMovement, null);
                    Debug.Log("Player movement state reset");
                }
            }
        }
    }
}

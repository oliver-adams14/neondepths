using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

// Level completion flag that tracks enemy count and triggers win condition
public class Flag : MonoBehaviour
{
    // Event triggered when player completes the level
    public static event Action LvlWon;

    // State tracking
    private bool hasWon = false;
    private bool eventFired = false;

    // UI references
    [SerializeField] private Canvas canvas;
    [SerializeField] private TMP_Text enemyTxt;
    
    // Enemy tracking
    private GameObject[] enemies;
    private int enemyCount;

    private HashSet<GameObject> deadEnemies = new HashSet<GameObject>();
    private void Start()
    {
        // Subscribe to level reset event
        LevelReset.LvlReset += ResetFlag;
        
        // Initialize enemy counter
        InitializeEnemyCount();
    }

    // Count all enemies in the level at start
    private void InitializeEnemyCount()
    {
        enemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = 0;
        
        foreach (GameObject enemy in enemies) 
        {
            enemyCount++;
        }
        
        // Update UI counter
        enemyTxt.text = enemyCount.ToString();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        LevelReset.LvlReset -= ResetFlag;
    }
    
    private void OnDisable()
    {
        // Prevent event handler leaks when disabled
        ResetInternalState();
    }
    
    private void ResetInternalState()
    {
        hasWon = false;
        eventFired = false;
        deadEnemies.Clear();
    }

    private void Update()
    {
        // Find player and make UI face them
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) {
            RotateTowardPlayer(player.transform.position);
        }

        // Check for enemies that have fallen out of the level (defeated)
        CheckForFallenEnemies();
    }
    
    // Check if any enemies have been defeated (fallen out of bounds)
    private void CheckForFallenEnemies()
    {
        foreach (GameObject enemy in enemies)
        {
            // Consider enemies below y=-150 as defeated
            if (enemy != null && enemy.transform.position.y <= -150f && !deadEnemies.Contains(enemy))
            {
                deadEnemies.Add(enemy);
                enemyCount--;
                enemyTxt.text = enemyCount.ToString();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only trigger win once
        if (hasWon || eventFired) return;

        // Check if all enemies have been defeated
        if (enemyCount == 0)
        {
            TriggerVictory();
        }
    }
    
    // Handle victory state when player reaches flag with all enemies defeated
    private void TriggerVictory()
    {
        // Show win screen
        UIManager.Instance.winScreenOn();
        hasWon = true;
        
        try
        {
            // Notify subscribers that the level is complete
            if (LvlWon != null)
            {
                eventFired = true;
                LvlWon.Invoke();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error invoking LvlWon event: {e.Message}\n{e.StackTrace}");
        }
    }

    // Reset the flag when level is restarted
    private void ResetFlag()
    {
        // Clear tracking state
        deadEnemies.Clear();
        hasWon = false;
        eventFired = false;
        
        // Reinitialize enemy count
        InitializeEnemyCount();
    }
    
    // Make UI elements face the player
    private void RotateTowardPlayer(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        directionToPlayer.y = 0f; // Keep UI upright (only rotate horizontally)
        
        Quaternion toRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        canvas.transform.parent.rotation = Quaternion.Slerp(
            canvas.transform.parent.rotation, toRotation, Time.deltaTime);
    }
}

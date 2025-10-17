using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;

// Enemy AI controller that handles shooting behavior, health tracking and UI
public class Enemy : MonoBehaviour
{
    [Header("Enemy Combat")]
    [SerializeField] private string typeOfEnemy;       // Determines shooting pattern (Red=circular, Green=direct)
    [SerializeField] private GameObject enemyBulletPrefab;
    [SerializeField] private Transform shootingPoint;
    [SerializeField] private float shotReload = 2f;    // Time between shots
    [SerializeField] private float attackRange = 10f;  // Distance at which enemy can detect player
    [SerializeField] private int damage = 1;

    [Header("Enemy Health")]
    [SerializeField] private int enemyHealth = 10;
    [SerializeField] private Image HealthBar;
    [SerializeField] private Canvas healthBarCanvas;
    [SerializeField] private GameObject holder;        // Container for health bar UI

    private int maxHealth;
    private float lastShot;
    private bool healthbarShowing = false;



    private void Start()
    {
        LevelReset.LvlReset += ResetEnemy;
        maxHealth = enemyHealth;
    }
    
    private void Update()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Make health bar face the player
        RotateHealthBarCanvas(player.transform.position);
        
        // Show health bar only when damaged
        if(enemyHealth != maxHealth)
        {
            healthbarShowing = true;
        }
        
        holder.SetActive(healthbarShowing);

        // Attack logic
        if (playerInAttackRange())
        {
            if(Time.time - lastShot > shotReload)
            {
                shoot(player.transform.position);
                lastShot = Time.time;
            }
        }
    }    // Reset enemy to initial state when level is restarted
    private void ResetEnemy()
    {
        healthbarShowing = false;
        enemyHealth = maxHealth;
        
        // Reset health bar visual
        if (HealthBar != null)
        {
            HealthBar.fillAmount = 1.0f;
        }
        
        // Make sure enemy is active
        if (gameObject != null)
        {
            gameObject.SetActive(true);
        }
        
        // Hide health bar initially
        if (holder != null)
        {
            holder.SetActive(false);
        }
    }
    private void OnDestroy()
    {
        LevelReset.LvlReset -= ResetEnemy;
    }

    // Check if player is within attack range
    private bool playerInAttackRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            return distanceToPlayer <= attackRange;
        }

        return false;
    }

    // Handle different shooting patterns based on enemy type
    private void shoot(Vector3 playerPosition)
    {
        if (typeOfEnemy == "Red")
        {
            ShootCircle(); // 360-degree attack pattern
        }
        else
        {
            // Direct targeted shot
            Vector3 direction = playerPosition - shootingPoint.position;
            GameObject bullet = Instantiate(enemyBulletPrefab, shootingPoint.position, Quaternion.LookRotation(direction));
            greenEnemyBullet bulletScript = bullet.GetComponent<greenEnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.SetDamage(damage);
            }
        }
    }

    // Handle enemy taking damage and update health bar UI
    public void TakeDamage(int damageAmount)
    {
        enemyHealth -= damageAmount;
        HealthBar.fillAmount = (float)enemyHealth / maxHealth;
        
        if (enemyHealth <= 0)
        {
            // Move enemy below the level to "defeat" it
            gameObject.transform.position = new Vector3(1, -150, 10);
        }
    }
    
    // Create a circular pattern of 8 bullets fired in all directions
    private void ShootCircle()
    {
        float angleStep = 360f / 8f;

        for (int i = 0; i < 8; i++)
        {
            float angle = i * angleStep;
            Vector3 shootDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            GameObject bullet = Instantiate(enemyBulletPrefab, shootingPoint.position, Quaternion.identity);
            bullet.transform.rotation = Quaternion.LookRotation(shootDirection);

            bullet.GetComponent<Rigidbody>().linearVelocity = shootDirection * 5f;
        }
    }
    
    // Keep health bar facing the player at all times
    private void RotateHealthBarCanvas(Vector3 playerPosition)
    {
        Vector3 directionToPlayer = playerPosition - transform.position;
        directionToPlayer.y = 0f; // Keep health bar level horizontally
        Quaternion toRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
        healthBarCanvas.transform.parent.rotation = Quaternion.Slerp(
            healthBarCanvas.transform.parent.rotation, toRotation, Time.deltaTime);
    }

}

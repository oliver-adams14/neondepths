using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Projectile fired by green enemies that moves in a straight line
// Uses event-based system to notify game when player is hit
public class greenEnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;    // Bullet speed in units per second
    
    private int bulletDamage;                      // Damage set by spawning enemy
    
    // Event triggered when bullet hits player
    public static event Action playerHit;
    private void Start()
    {
        // Initialize bullet velocity and set lifetime
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, 2f);  // Self-destruct after 2 seconds
    }

    // Sets damage value from spawning enemy
    public void SetDamage(int damage)
    {
        bulletDamage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Player hit - trigger event and destroy bullet
            if (playerHit != null)
            {
                playerHit();
            }
            Destroy(gameObject);
        }
        else if (!other.CompareTag("Enemy"))
        {
            // Hit environment - destroy bullet
            Destroy(gameObject);
        }
    }
}

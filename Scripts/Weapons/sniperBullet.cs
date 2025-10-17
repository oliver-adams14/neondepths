using UnityEngine;

// High-damage sniper bullet projectile
public class SniperBullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 2f;
    private int damage = 10;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }
    
    // Set damage value for this projectile
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Ignore collisions with player
        if (!(collision.gameObject.CompareTag("Player")))
        {
            // Apply damage to enemies
            if(collision.gameObject.CompareTag("Enemy"))
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
            
            // Destroy bullet on impact
            Destroy(gameObject);
        }
    }
}

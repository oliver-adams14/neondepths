using UnityEngine;

// Handles damage and knockback when player dash ability hits enemies
public class PlayerDashCollision : MonoBehaviour
{
    private WeaponManager weaponManager;

    private void Start()
    {
        // Find the weapon manager in the scene
        weaponManager = FindAnyObjectByType<WeaponManager>();

        if (weaponManager == null)
        {
            Debug.LogError("PlayerDashCollision could not find WeaponManager!");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (weaponManager != null && weaponManager.IsDashing())
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Apply dash damage to enemy
                enemy.TakeDamage(weaponManager.GetDashDamage());

                // Add knockback force to push the enemy
                if (collision.gameObject.TryGetComponent<Rigidbody>(out Rigidbody enemyRb))
                {
                    Vector3 dashDirection = (collision.transform.position - transform.position).normalized;
                    dashDirection.y = 0f;
                    dashDirection.Normalize();
                    enemyRb.AddForce(dashDirection * 10f, ForceMode.Impulse);
                }
            }
        }
    }

    // Handle character controller collisions with enemies
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (weaponManager != null && weaponManager.IsDashing())
        {
            Enemy enemy = hit.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Apply dash damage to enemy hit by character controller
                enemy.TakeDamage(weaponManager.GetDashDamage());
            }
        }
    }
}
using UnityEngine;

// Projectile script for bullets and rockets with object pool integration
public class PistolBullet : MonoBehaviour
{
    [SerializeField] private float bulletLifetime = 5f;
    [SerializeField] private GameObject impactEffectPrefab;
    
    private int damage;
    private bool isHitscanVisual = false;
    private Vector3 targetPoint;
    private bool willHitTarget = false;
    private string poolName = "basic";

    // Reset bullet state when retrieved from object pool
    private void OnEnable()
    {
        // Reset state when retrieved from pool
        isHitscanVisual = false;
        willHitTarget = false;
        
        // Re-enable collider if it was disabled
        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = true;
        
        // Cancel any existing invoke and start new one
        CancelInvoke(nameof(ReturnToPool));
        Invoke(nameof(ReturnToPool), bulletLifetime);
    }

    // Cancel lifetime timers when returned to pool
    private void OnDisable()
    {
        CancelInvoke(nameof(ReturnToPool));
    }

    // Set damage value for this projectile
    public void SetDamage(int newDamage)
    {
        damage = newDamage;
    }
    
    // Set which pool this bullet belongs to for proper return
    public void SetPoolName(string name)
    {
        poolName = name;
    }
    
    // Configure bullet for hitscan visual mode (cosmetic only)
    public void SetHitscanMode(bool isVisualOnly, Vector3 hitPoint, bool willHit)
    {
        isHitscanVisual = isVisualOnly;
        targetPoint = hitPoint;
        willHitTarget = willHit;
        
        // If this is just a visual bullet, disable its collider
        if (isVisualOnly)
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
                col.enabled = false;
        }
    }
    
    // Handle collision with enemies or environment
    private void OnCollisionEnter(Collision collision)
    {
        // Only apply damage if not in hitscan mode
        if (!isHitscanVisual)
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // Create impact effect
        CreateImpactEffect(collision.contacts[0].point, collision.contacts[0].normal, collision.gameObject);
        
        // Return to pool instead of destroying
        ReturnToPool();
    }
    
    // Create visual effects at the impact point
    public void CreateImpactEffect(Vector3 position, Vector3 normal, GameObject hitObject)
    {
        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
        }
        
        // Different effects based on what was hit
        if (hitObject.CompareTag("Enemy"))
        {
            // Enemy hit effect
        }
        else
        {
            // Environment hit effect
        }
    }
    
    // Return this projectile to its object pool for reuse
    private void ReturnToPool()
    {
        // Reset velocity before returning to pool
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        ProjectilePoolManager.Instance.ReturnProjectile(poolName, this);
    }
}
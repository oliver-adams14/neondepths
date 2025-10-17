using UnityEngine;
using System.Collections;

// Manages object pools for different projectile types to improve performance
public class ProjectilePoolManager : MonoBehaviour
{
    // Singleton instance
    private static ProjectilePoolManager instance;
    public static ProjectilePoolManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ProjectilePoolManager");
                instance = go.AddComponent<ProjectilePoolManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 20;
    [SerializeField] private int maxPoolSize = 100;

    // Projectile pools by type
    private ObjectPool<PistolBullet> basicBulletPool;
    private ObjectPool<PistolBullet> pistolBulletPool;
    private ObjectPool<PistolBullet> arBulletPool;
    private ObjectPool<PistolBullet> sniperBulletPool;
    private ObjectPool<PistolBullet> shotgunBulletPool;
    private ObjectPool<PistolBullet> burstBulletPool;
    private ObjectPool<PistolBullet> rocketPool;

    private void Awake()
    {
        // Singleton pattern implementation
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Initialize a pool for a specific projectile type
    public void InitializePool(string poolName, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogError($"Cannot initialize pool '{poolName}': prefab is null");
            return;
        }

        ObjectPool<PistolBullet> pool = new ObjectPool<PistolBullet>(
            prefab, 
            initialPoolSize, 
            maxPoolSize, 
            transform
        );

        switch (poolName.ToLower())
        {
            case "basic":
                basicBulletPool = pool;
                break;
            case "pistol":
                pistolBulletPool = pool;
                break;
            case "ar":
                arBulletPool = pool;
                break;
            case "sniper":
                sniperBulletPool = pool;
                break;
            case "shotgun":
                shotgunBulletPool = pool;
                break;
            case "burst":
                burstBulletPool = pool;
                break;
            case "rocket":
                rocketPool = pool;
                break;
            default:
                Debug.LogWarning($"Unknown pool name: {poolName}");
                break;
        }
    }

    // Get a projectile from the appropriate pool
    public PistolBullet GetProjectile(string poolName)
    {
        ObjectPool<PistolBullet> pool = GetPool(poolName);
        
        if (pool == null)
        {
            Debug.LogError($"Pool '{poolName}' not initialized. Call InitializePool first.");
            return null;
        }

        return pool.Get();
    }

    // Return a projectile to its pool
    public void ReturnProjectile(string poolName, PistolBullet projectile)
    {
        ObjectPool<PistolBullet> pool = GetPool(poolName);
        
        if (pool == null)
        {
            Debug.LogError($"Pool '{poolName}' not found. Destroying projectile instead.");
            if (projectile != null)
                Destroy(projectile.gameObject);
            return;
        }

        pool.Return(projectile);
    }

    // Return a projectile after a delay
    public void ReturnProjectileAfterDelay(string poolName, PistolBullet projectile, float delay)
    {
        ObjectPool<PistolBullet> pool = GetPool(poolName);
        
        if (pool == null)
        {
            Debug.LogError($"Pool '{poolName}' not found.");
            return;
        }

        pool.ReturnAfterDelay(projectile, delay, this);
    }

    // Get the pool for a given name
    private ObjectPool<PistolBullet> GetPool(string poolName)
    {
        switch (poolName.ToLower())
        {
            case "basic": return basicBulletPool;
            case "pistol": return pistolBulletPool;
            case "ar": return arBulletPool;
            case "sniper": return sniperBulletPool;
            case "shotgun": return shotgunBulletPool;
            case "burst": return burstBulletPool;
            case "rocket": return rocketPool;
            default: return null;
        }
    }

    // Return all active projectiles from all pools
    public void ReturnAllProjectiles()
    {
        basicBulletPool?.ReturnAll();
        pistolBulletPool?.ReturnAll();
        arBulletPool?.ReturnAll();
        sniperBulletPool?.ReturnAll();
        shotgunBulletPool?.ReturnAll();
        burstBulletPool?.ReturnAll();
        rocketPool?.ReturnAll();
    }

    // Log pool statistics for debugging
    public void LogPoolStats()
    {
        LogSinglePoolStats("Basic", basicBulletPool);
        LogSinglePoolStats("Pistol", pistolBulletPool);
        LogSinglePoolStats("AR", arBulletPool);
        LogSinglePoolStats("Sniper", sniperBulletPool);
        LogSinglePoolStats("Shotgun", shotgunBulletPool);
        LogSinglePoolStats("Burst", burstBulletPool);
        LogSinglePoolStats("Rocket", rocketPool);
    }

    private void LogSinglePoolStats(string name, ObjectPool<PistolBullet> pool)
    {
        if (pool == null)
        {
            Debug.Log($"{name} Pool: Not initialized");
            return;
        }

        var stats = pool.GetStats();
        Debug.Log($"{name} Pool - Available: {stats.available}, Active: {stats.active}, Total: {stats.total}");
    }
}

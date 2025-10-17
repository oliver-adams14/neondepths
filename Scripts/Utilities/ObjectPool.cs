using System.Collections.Generic;
using UnityEngine;

// Generic object pooling system to reduce instantiation overhead and garbage collection
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly GameObject prefab;
    private readonly Transform poolParent;
    private readonly Queue<T> availableObjects = new Queue<T>();
    private readonly HashSet<T> activeObjects = new HashSet<T>();
    private readonly int initialSize;
    private readonly int maxSize;

    // Creates a new object pool
    public ObjectPool(GameObject prefab, int initialSize = 10, int maxSize = 100, Transform poolParent = null)
    {
        this.prefab = prefab;
        this.initialSize = initialSize;
        this.maxSize = maxSize;
        
        // Create a parent object to keep the hierarchy clean
        if (poolParent == null)
        {
            GameObject poolContainer = new GameObject($"{prefab.name}_Pool");
            this.poolParent = poolContainer.transform;
        }
        else
        {
            this.poolParent = poolParent;
        }

        // Pre-instantiate initial objects
        for (int i = 0; i < initialSize; i++)
        {
            CreateNewObject();
        }
    }

    // Gets an object from the pool or creates a new one if needed
    public T Get()
    {
        T obj;

        if (availableObjects.Count > 0)
        {
            obj = availableObjects.Dequeue();
        }
        else
        {
            // Check if we've hit the max size limit
            if (maxSize > 0 && (availableObjects.Count + activeObjects.Count) >= maxSize)
            {
                Debug.LogWarning($"ObjectPool for {prefab.name} has reached max size of {maxSize}. Reusing oldest object.");
                // Create anyway for simplicity
            }
            
            obj = CreateNewObject();
        }

        activeObjects.Add(obj);
        obj.gameObject.SetActive(true);
        return obj;
    }

    // Returns an object to the pool for reuse
    public void Return(T obj)
    {
        if (obj == null) return;

        if (!activeObjects.Contains(obj))
        {
            Debug.LogWarning($"Trying to return object {obj.name} that wasn't tracked by the pool.");
            return;
        }

        activeObjects.Remove(obj);
        obj.gameObject.SetActive(false);
        obj.transform.SetParent(poolParent);
        availableObjects.Enqueue(obj);
    }

    // Returns an object to the pool after a delay
    public void ReturnAfterDelay(T obj, float delay, MonoBehaviour coroutineRunner)
    {
        coroutineRunner.StartCoroutine(ReturnAfterDelayCoroutine(obj, delay));
    }

    private System.Collections.IEnumerator ReturnAfterDelayCoroutine(T obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Return(obj);
    }

    // Creates a new pooled object
    private T CreateNewObject()
    {
        GameObject instance = Object.Instantiate(prefab, poolParent);
        instance.SetActive(false);
        T component = instance.GetComponent<T>();
        
        if (component == null)
        {
            Debug.LogError($"Prefab {prefab.name} does not have component {typeof(T).Name}");
        }

        availableObjects.Enqueue(component);
        return component;
    }

    // Returns all active objects to the pool
    public void ReturnAll()
    {
        var objectsToReturn = new List<T>(activeObjects);
        foreach (var obj in objectsToReturn)
        {
            Return(obj);
        }
    }

    // Gets the current statistics of the pool
    public (int available, int active, int total) GetStats()
    {
        return (availableObjects.Count, activeObjects.Count, availableObjects.Count + activeObjects.Count);
    }
}

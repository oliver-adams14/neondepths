using UnityEngine;

// Controls floating and spinning animation for weapon pickups
public class WeaponIdolBob : MonoBehaviour
{
    [SerializeField] private float bobFrequency = 2f;
    [SerializeField] private float bobAmplitude = 0.2f;
    [SerializeField] private float rotationSpeed = 50f;

    private Vector3 startPos;
    private float bobTime;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Bobbing up and down motion
        bobTime += Time.deltaTime;
        float newY = startPos.y + Mathf.Sin(bobTime * bobFrequency) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Spinning rotation
        transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);
    }
}
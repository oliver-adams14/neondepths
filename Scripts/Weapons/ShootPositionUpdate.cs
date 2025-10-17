using UnityEngine;

// Updates shooting position to match player camera direction
public class ShootPositionUpdate : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;

    void Update()
    {
        // Align shooting direction with camera's horizontal rotation
        transform.rotation = Quaternion.Euler(0f, playerCamera.transform.eulerAngles.y, 0f);
    }
}

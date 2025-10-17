using UnityEngine;
using System.Collections;

// Bounce pad ability for AR weapon that propels the player upward when stepped on
public class ARAbility : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float launchForce = 20f;
    [SerializeField] private float expansionSpeed = 5f;
    [SerializeField] private Vector3 targetScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private float lifetime = 10f;

    private bool isDeployed = false;
    private Rigidbody rb;
    private Collider col;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // If the pad is already deployed, bounce the player or do nothing
        if (isDeployed)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                BouncePlayer(collision.gameObject);
            }
            return;
        }

        // Deploy on the first non-player object hit
        if (!collision.gameObject.CompareTag("Player"))
        {
            isDeployed = true;

            // Stop the pad from moving
            rb.isKinematic = true;

            // Align the pad to the surface it hit
            ContactPoint contact = collision.contacts[0];
            transform.rotation = Quaternion.LookRotation(contact.normal);

            // Attach it securely using a FixedJoint
            FixedJoint joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = collision.rigidbody;
            joint.anchor = contact.point;

            // Start the expansion
            StartCoroutine(Expand());
        }
    }

    // Launch player upward when they step on the pad
    private void BouncePlayer(GameObject playerObject)
    {
        Rigidbody playerRb = playerObject.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Use pad's forward direction for launch
            Vector3 launchDirection = transform.forward;

            // Reset vertical velocity for consistent bounce height
            playerRb.linearVelocity = new Vector3(playerRb.linearVelocity.x, 0, playerRb.linearVelocity.z);
            
            // Apply the launch force
            playerRb.AddForce(launchDirection * launchForce, ForceMode.Impulse);
        }
    }

    // Gradually expand the pad to full size
    private IEnumerator Expand()
    {
        Vector3 initialScale = transform.localScale;
        float journey = 0f;

        while (journey < 1f)
        {
            journey += Time.deltaTime * expansionSpeed;
            transform.localScale = Vector3.Lerp(initialScale, targetScale, journey);
            yield return null;
        }
        transform.localScale = targetScale;
    }
}

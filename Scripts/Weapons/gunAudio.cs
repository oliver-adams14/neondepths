using UnityEngine;

// Handles audio playback for different weapon types
public class GunAudio : MonoBehaviour
{
    [SerializeField] private AudioClip pistolShot;
    [SerializeField] private AudioClip arShot;
    
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Play pistol firing sound effect
    public void PlayPistolSound()
    {
        if (audioSource != null && pistolShot != null)
        {
            audioSource.PlayOneShot(pistolShot);
        }
    }

    // Play assault rifle firing sound effect
    public void PlayARSound()
    {
        if (audioSource != null && arShot != null)
        {
            audioSource.PlayOneShot(arShot);
        }
    }
}

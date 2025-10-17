using UnityEngine;

// Controls in-game settings menu visibility and game pausing
public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;

    // Resume gameplay when settings are closed
    public void Resume()
    {
        settingsPanel.SetActive(false);
        Time.timeScale = 1.0f;
    }
}

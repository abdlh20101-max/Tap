using UnityEngine;
using UnityEngine.UI;

public class PerfectTapSettingsSimple : MonoBehaviour
{
    [Header("--- SOUND SETTINGS ---")]
    public bool soundOn = true;
    public Button muteButton;

    [Header("--- BACKGROUND ---")]
    public Image backgroundImage;
    public Color easyColor = Color.green;
    public Color hardColor = Color.red;

    [Header("--- DIFFICULTY ---")]
    public int difficulty = 1; // 1 سهل - 2 صعب

    // -------- FUNCTIONS --------

    public void ToggleSound()
    {
        soundOn = !soundOn;
        AudioListener.volume = soundOn ? 1f : 0f;
    }

    public void SetEasy()
    {
        difficulty = 1;
        if (backgroundImage != null)
            backgroundImage.color = easyColor;
    }

    public void SetHard()
    {
        difficulty = 2;
        if (backgroundImage != null)
            backgroundImage.color = hardColor;
    }
}
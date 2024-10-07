using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TextFunction : MonoBehaviour
{
    public TMP_Text gameDurationText;
    private float timerDuration = 200f;
    private float timeRemaining;

    void Start()
    {
        timeRemaining = timerDuration; // Initialize the timer
        UpdateTimerText(); // Set initial timer text
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime; // Decrease time remaining
            UpdateTimerText(); // Update the text with remaining time

            if (timeRemaining <= 0)
            {
                timeRemaining = 0; // Ensure it doesn't go negative
                UpdateTimerText(); // Update the text to show "0s"
                // WinScene(); // Call method to transition to win scene
            }
        }
    }

    void UpdateTimerText()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);
        gameDurationText.text = $"Time: {secondsRemaining}s"; // Update text format
    }

    void WinScene()
    {
        // Load the win scene. Ensure you have added the scene to the build settings.
        SceneManager.LoadScene("winScene");
    }
}

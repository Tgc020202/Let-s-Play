using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameViewTextBehaviour : MonoBehaviour
{
    public Text gameDuration;
    public Text roleText;

    private float timerDuration = 20f;
    private float timeRemaining;

    void Start()
    {
        timeRemaining = timerDuration;
        UpdateTimerText();
    }

    void Update()
    {
        // Decrease the timer if time is remaining
        if (timeRemaining > 0)
        {
            timeRemaining = Mathf.Max(0, timeRemaining - Time.deltaTime);
            UpdateTimerText();

            // If time runs out, call EndGame
            if (timeRemaining == 0)
            {
                EndGame();
            }
        }
    }

    // Method to update the timer text
    void UpdateTimerText()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);
        gameDuration.text = $"{secondsRemaining}s";
    }

    // Method to handle the end of the game and load the appropriate scene
    void EndGame()
    {
        // TODO: Staff need time to 0 for win, Boss need total players to 0 for win
        VariableHolder.isBossWin = false;
        if (roleText.text == "Worker")
        {
            SceneManager.LoadScene("EndGameScene");
            Debug.Log("Worker");
        }
        else if (roleText.text == "Boss")
        {
            SceneManager.LoadScene("EndGameScene");
            Debug.Log("Boss");
        }
        else
        {
            Debug.LogWarning("Unknown role: " + roleText.text);
        }
    }
}

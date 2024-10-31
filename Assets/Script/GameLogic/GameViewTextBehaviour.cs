using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameViewTextBehaviour : NetworkBehaviour
{
    public Text gameDuration;
    public Text roleText;

    private NetworkVariable<float> timerDuration = new NetworkVariable<float>(200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server); // Timer will be managed by the server
    private float timeRemaining;

    void Start()
    {
        // Initialize the timer for clients (they'll get synced updates from the server)
        timeRemaining = timerDuration.Value;
        UpdateTimerText();
    }

    void Update()
    {
        if (IsServer)
        {
            // Server controls the timer and updates it
            if (timeRemaining > 0)
            {
                timeRemaining = Mathf.Max(0, timeRemaining - Time.deltaTime);
                timerDuration.Value = timeRemaining; // Update NetworkVariable with the remaining time

                if (timeRemaining == 0)
                {
                    EndGameServerRpc(); // Call the end game on the server
                }
            }
        }
        else
        {
            // Clients just read the synchronized timerDuration from the server
            timeRemaining = timerDuration.Value;
        }

        UpdateTimerText(); // Update the UI for all clients
    }

    // Method to update the timer text on the UI
    void UpdateTimerText()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);
        gameDuration.text = $"{secondsRemaining}s";
    }

    // Method to handle the end of the game
    [ServerRpc]
    void EndGameServerRpc()
    {
        // Logic for determining the winner
        VariableHolder.isBossWin = false; // Placeholder logic; adjust as needed

        // Based on the player's role, decide the outcome
        if (roleText.text == "Worker")
        {
            EndGameClientRpc("Worker"); // Send an end game call to clients
        }
        else if (roleText.text == "Boss")
        {
            EndGameClientRpc("Boss"); // Send an end game call to clients
        }
        else
        {
            Debug.LogWarning("Unknown role: " + roleText.text);
        }
    }

    [ClientRpc]
    void EndGameClientRpc(string role)
    {
        // Load the end game scene on all clients
        SceneManager.LoadScene("EndGameScene");
        Debug.Log(role + " wins!");
    }
}

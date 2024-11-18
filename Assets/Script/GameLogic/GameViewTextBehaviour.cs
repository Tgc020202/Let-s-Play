using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameViewTextBehaviour : NetworkBehaviour
{
    public Text gameDuration;
    public Text roleText;

    private AudioSource BackgroundMusic;
    private NetworkVariable<float> timerDuration = new NetworkVariable<float>(200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float timeRemaining;

    void Start()
    {
        BackgroundMusic = GameObject.Find("AudioManager/BossBackgroundMusic").GetComponent<AudioSource>();
        timeRemaining = timerDuration.Value;
        UpdateTimerText();
    }

    void Update()
    {
        if (IsServer)
        {
            if (timeRemaining > 0)
            {
                timeRemaining = Mathf.Max(0, timeRemaining - Time.deltaTime);
                timerDuration.Value = timeRemaining;

                if (timeRemaining == 0)
                {
                    EndGameServerRpc(false);
                }
            }
        }
        else
        {
            timeRemaining = timerDuration.Value;
        }

        UpdateTimerText();
    }

    // Method to update the timer text on the UI
    void UpdateTimerText()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);
        gameDuration.text = $"{secondsRemaining}s";
    }

    // Method to handle the end of the game
    [ServerRpc]
    public void EndGameServerRpc(bool isBossWin)
    {
        EndGameClientRpc(isBossWin);
    }

    [ClientRpc]
    void EndGameClientRpc(bool isBossWin)
    {
        BackgroundMusic.Stop();
        VariableHolder.isBossWin = isBossWin;

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        Debug.Log("Destroy already!!!");

        SceneManager.LoadScene("EndGameScene");
    }
}

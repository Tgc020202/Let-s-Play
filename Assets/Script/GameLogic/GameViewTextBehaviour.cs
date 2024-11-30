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
        BackgroundMusic.Play(0);

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

    void UpdateTimerText()
    {
        int secondsRemaining = Mathf.CeilToInt(timeRemaining);
        gameDuration.text = $"{secondsRemaining}s";
    }

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

    // Reduce time duration when task completed
    [ServerRpc(RequireOwnership = false)]
    public void ReduceTimeDurationServerRpc(float seconds)
    {
        timeRemaining = Mathf.Max(0, timeRemaining - seconds);
        timerDuration.Value = timeRemaining;

        if (timeRemaining <= 0)
        {
            EndGameServerRpc(false);
        }
    }
}

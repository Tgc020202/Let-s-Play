using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameViewTextBehaviour : NetworkBehaviour
{
    // UI Components
    public Text gameDuration;
    public Text roleText;

    // Audio
    private AudioSource BackgroundMusic;

    // GameObjects
    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;

    // Defines
    private float timeRemaining;
    private bool hasMusicStarted = false;

    // Network Variables
    public NetworkVariable<float> timerDuration = new NetworkVariable<float>(200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    void Start()
    {
        timeRemaining = timerDuration.Value;
        UpdateTimerText();
    }

    void Update()
    {
        if (!hasMusicStarted)
        {
            if (BossControllerUI.activeSelf)
            {
                BackgroundMusic = GameObject.FindObjectOfType<BossAudioController>()?.BackgroundMusic;
            }
            else if (WorkerControllerUI.activeSelf)
            {
                BackgroundMusic = GameObject.FindObjectOfType<WorkerAudioController>()?.BackgroundMusic;
            }

            if (BackgroundMusic != null)
            {
                BackgroundMusic.Play();
                hasMusicStarted = true;
            }
        }

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
        if (BackgroundMusic != null)
        {
            BackgroundMusic.Stop();
        }

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        if (isBossWin)
        {
            SceneManager.LoadScene("EndGameSceneBossWin");
        }
        else
        {
            SceneManager.LoadScene("EndGameSceneWorkerWin");
        }
    }

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

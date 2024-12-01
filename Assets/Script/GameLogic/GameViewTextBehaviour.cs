using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameViewTextBehaviour : NetworkBehaviour
{
    public Text gameDuration;
    public Text roleText;

    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;

    private AudioSource BackgroundMusic;

    public NetworkVariable<float> timerDuration = new NetworkVariable<float>(200f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private float timeRemaining;
    private bool hasMusicStarted = false;

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

        RoomManager.Instance.isBossWin = isBossWin;

        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        Debug.Log("Destroy already!!!");

        SceneManager.LoadScene("EndGameScene");
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

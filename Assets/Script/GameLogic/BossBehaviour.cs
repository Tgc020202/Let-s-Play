using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;

public class BossBehaviour : MonoBehaviour
{
    private NetworkObject playerNetworkObject;
    public PlayerMovement playerMovement;
    public GameObject CountDownUI;
    public GameObject BossControllerUI;
    public Text message;
    public Text countdownText;
    public GameViewTextBehaviour gameViewTextBehaviour;

    private bool controlsEnabled = false;
    private bool isBoss = false;

    void Start()
    {
        if (CountDownUI == null || message == null || countdownText == null)
        {
            Debug.LogWarning("UI elements are not properly assigned in the Inspector!");
        }

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void Update()
    {
        AssignRole();
        if (gameViewTextBehaviour == null) return;

        float timeRemaining = gameViewTextBehaviour.timerDuration.Value;

        if (isBoss)
        {
            if (timeRemaining > 189f)
            {
                DisableBossControls();
                UpdateCountdownText(200f - timeRemaining);
            }
            else if (!controlsEnabled)
            {
                EnableBossControls();
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        StartCoroutine(WaitForPlayerObject());
    }

    private IEnumerator WaitForPlayerObject()
    {
        while (NetworkManager.Singleton.LocalClient == null || NetworkManager.Singleton.LocalClient.PlayerObject == null)
        {
            yield return null;
        }

        playerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>();
        playerMovement = playerNetworkObject.GetComponent<PlayerMovement>();

        if (playerMovement == null)
        {
            Debug.LogError("PlayerMovement script is not attached to the PlayerObject!");
        }
    }

    private void AssignRole()
    {
        if (BossControllerUI.activeSelf)
        {
            isBoss = true;
        }
        else
        {
            isBoss = false;
        }
    }

    void UpdateCountdownText(float elapsed)
    {
        int remainingTime = Mathf.CeilToInt(10f - elapsed);

        if (remainingTime >= -1)
        {
            if (remainingTime > 0)
            {
                countdownText.text = remainingTime.ToString();
            }
            else
            {
                countdownText.text = "Let's go to catch all workers!";
                message.text = "";
            }
        }
        else
        {
            countdownText.text = "";
        }
    }

    void DisableBossControls()
    {
        if (playerMovement != null && playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            playerMovement.enabled = false;
        }

        if (CountDownUI != null)
        {
            CountDownUI.SetActive(true);
        }

        controlsEnabled = false;
    }

    void EnableBossControls()
    {
        if (playerMovement != null && playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            playerMovement.enabled = true;
        }

        if (CountDownUI != null)
        {
            CountDownUI.SetActive(false);
        }

        controlsEnabled = true;
    }
}

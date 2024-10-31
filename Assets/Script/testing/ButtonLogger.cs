using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Collections;  // Add this to use IEnumerator

public class ButtonLogger : MonoBehaviour
{
    public Button logButton; // Reference to the UI Button
    private NetworkObject playerNetworkObject; // Dynamically reference the network player GameObject
    public PlayerMovement playerMovement; // Reference to the PlayerMovement script
    // private bool canUseRunButton = true;
    public Button runButton;
    private Text runButtonText;

    void Start()
    {
        if (logButton != null)
        {
            logButton.onClick.AddListener(OnButtonClick);
        }
        runButtonText = runButton.GetComponentInChildren<Text>();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            playerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>();
            playerMovement = playerNetworkObject.GetComponent<PlayerMovement>();
        }
        else
        {
            Debug.LogError("Player object not found after spawn!");
        }
    }

    void OnButtonClick()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            Debug.Log("Player " + playerNetworkObject.NetworkObjectId + " clicked the button!");
            StartCoroutine(SpeedBoost());
        }
        else
        {
            Debug.LogError("Button clicked by non-owner or player object not set!");
        }
    }

    IEnumerator SpeedBoost()
    {
        // canUseRunButton = false;

        playerMovement.IncreaseSpeedServerRpc(true);

        // Countdown from 20 seconds
        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s"; // Update button text to show countdown
            yield return new WaitForSeconds(1); // Wait 1 second per count
            if (i == 16)
            {
                playerMovement.IncreaseSpeedServerRpc(false);
            }
        }
        runButtonText.text = "Run";
        // canUseRunButton = true;
    }
}

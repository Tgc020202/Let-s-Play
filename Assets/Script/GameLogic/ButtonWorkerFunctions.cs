using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;
public class ButtonWorkerFunctions : MonoBehaviour
{
    private NetworkObject playerNetworkObject;
    public PlayerMovement playerMovement;
    public Button runButton;
    public Button redButton;
    public Button greenButton;
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;

    private bool isPlayerStop = false;
    private bool canUseRunButton = true;
    private Text runButtonText;
    private float timer = 5f;

    void Start()
    {
        // Assign button click listeners
        runButton.onClick.AddListener(OnRunButtonClicked);
        redButton.onClick.AddListener(OnRedButtonClicked);
        greenButton.onClick.AddListener(OnGreenButtonClicked);

        runButtonText = runButton.GetComponentInChildren<Text>();
        GameViewUI.SetActive(false);
        MapDesign.SetActive(false);
        GuidanceUI.SetActive(true);
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    void Update()
    {
        // Press Space key to skip guidance
        if (Input.GetKey(KeyCode.Space)) OnSkipGuidanceUI();

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            OnSkipGuidanceUI();
        }
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

    void OnRunButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canUseRunButton && !isPlayerStop)
        {
            Debug.Log("Player " + playerNetworkObject.NetworkObjectId + " clicked the run button!");
            StartCoroutine(SpeedBoost());
        }
    }

    void OnRedButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            Debug.Log("Player " + playerNetworkObject.NetworkObjectId + " clicked the stop button!");
            isPlayerStop = true;
            playerMovement.StopServerRpc(true);
        }
    }

    void OnGreenButtonClicked()
    {
        Debug.Log("Help: Use the run button to speed up, red button to stop, green button for help.");
    }

    IEnumerator SpeedBoost()
    {
        canUseRunButton = false;

        playerMovement.IncreaseSpeedServerRpc(true);

        // Countdown from 20 seconds
        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s"; // Update button text to show countdown
            yield return new WaitForSeconds(1); // Wait 1 second per count
            if (i == 16 && !isPlayerStop)
            {
                playerMovement.IncreaseSpeedServerRpc(false);
            }
        }
        runButtonText.text = "Run";
        canUseRunButton = true;
    }

    void OnSkipGuidanceUI()
    {
        MapDesign.SetActive(true);
        GameViewUI.SetActive(true);
        GuidanceUI.SetActive(false);
    }
}

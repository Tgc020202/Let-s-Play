using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class ButtonWorkerFunctions : MonoBehaviour
{
    private NetworkObject playerNetworkObject;
    public PlayerMovement playerMovement;
    public CollisionTriggerDisplay collisionTriggerDisplay;
    public TaskManager taskManager;
    public Button runButton;
    public Button redButton;
    public Button greenButton;
    public Button completeTaskButton;   // test
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;

    private bool canUseRunButton = true;
    private Text runButtonText;
    private float timer = 5f;

    private bool canHelpPlayer = false;
    private GameObject targetPlayer;

    void Start()
    {
        runButton.onClick.AddListener(OnRunButtonClicked);
        redButton.onClick.AddListener(OnRedButtonClicked);
        greenButton.onClick.AddListener(OnGreenButtonClicked);
        runButtonText = runButton.GetComponentInChildren<Text>();

        GameViewUI.SetActive(false);
        MapDesign.SetActive(false);
        GuidanceUI.SetActive(true);
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

        // Test button for completing the task
        completeTaskButton.onClick.AddListener(OnCompleteTaskClicked);
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Space)) OnSkipGuidanceUI();

        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            OnSkipGuidanceUI();
        }

        if (playerMovement != null)
        {
            redButton.interactable = playerMovement.enabled;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
        {
            playerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>();
            playerMovement = playerNetworkObject.GetComponent<PlayerMovement>();
            collisionTriggerDisplay = playerNetworkObject.GetComponent<CollisionTriggerDisplay>();
        }
        else
        {
            Debug.LogError("Player object not found after spawn!");
        }
    }

    // OnClick for the Run button
    void OnRunButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canUseRunButton)
        {
            var playerManager = playerNetworkObject.GetComponent<PlayerManager>();
            if (playerManager != null && !playerManager.isImmuneToCatch)
            {
                // Task 2: Run Button Press
                taskManager.RunButtonPressed();
                StartCoroutine(SpeedBoost());
            }
            else
            {
                Debug.Log("Speed boost is disabled due to immunity.");
            }
        }
    }

    // OnClick for the Red button
    void OnRedButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            SetImmunityServerRpc(playerNetworkObject.NetworkObjectId, true);
            playerMovement.enabled = false;

            // Decrease the worker count when a player is caught
            GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(-1);

            // Task 1: Red Button Press
            taskManager.RedButtonPressed();
        }
    }

    // OnClick for the Green button (helping another player)
    void OnGreenButtonClicked()
    {
        targetPlayer = collisionTriggerDisplay.targetPlayer;
        canHelpPlayer = collisionTriggerDisplay.canHelpPlayer;

        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canHelpPlayer && targetPlayer != null)
        {
            HelpPlayerServerRpc(targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
            // Increase the worker count when a player is helped
            GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(+1);
        }
    }

    // Coroutine to handle the speed boost duration
    IEnumerator SpeedBoost()
    {
        canUseRunButton = false;
        playerMovement.IncreaseSpeedServerRpc(true);  // Server call to increase speed

        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s";
            yield return new WaitForSeconds(1);
            if (i == 16)
            {
                playerMovement.IncreaseSpeedServerRpc(false);  // Disable speed after 16 seconds
            }
        }
        runButtonText.text = "Run";
        canUseRunButton = true;
    }

    // Skips the guidance UI after a set time or button press
    void OnSkipGuidanceUI()
    {
        MapDesign.SetActive(true);
        GameViewUI.SetActive(true);
        GuidanceUI.SetActive(false);
    }

    // Server RPC to set immunity for a player
    [ServerRpc(RequireOwnership = false)]
    public void SetImmunityServerRpc(ulong targetPlayerId, bool enabled)
    {
        var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];
        if (targetNetworkObject != null)
        {
            var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();
            if (targetPlayerManager != null)
            {
                targetPlayerManager.SetImmunityServerRpc(enabled);
            }
        }
    }

    // Server RPC to help another player
    [ServerRpc(RequireOwnership = false)]
    public void HelpPlayerServerRpc(ulong targetPlayerId)
    {
        var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];
        if (targetNetworkObject != null)
        {
            var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();
            if (targetPlayerManager != null)
            {
                targetPlayerManager.SetMovementEnabledServerRpc(true);
                targetPlayerManager.SetImmunityServerRpc(false);
            }
        }
    }

    // Test function to simulate completing a task (can be triggered manually)
    void OnCompleteTaskClicked()
    {
        var playerNetworkObject = NetworkManager.Singleton.LocalClient?.PlayerObject.GetComponent<NetworkObject>();
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            Debug.Log("Worker completed the task!");
            taskManager.CompleteTaskServerRpc();
        }
    }
}

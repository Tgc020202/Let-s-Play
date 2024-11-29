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

        // test
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

    void OnRunButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canUseRunButton)
        {
            var playerManager = playerNetworkObject.GetComponent<PlayerManager>();
            if (playerManager != null && !playerManager.isImmuneToCatch)
            {
                StartCoroutine(SpeedBoost());
            }
            else
            {
                Debug.Log("Speed boost is disabled due to immunity.");
            }
        }
    }

    void OnRedButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            SetImmunityServerRpc(playerNetworkObject.NetworkObjectId, true);
            playerMovement.enabled = false;

            // Decrease the worker count when a player is caught
            GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(-1);
        }
    }

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

    IEnumerator SpeedBoost()
    {
        canUseRunButton = false;
        playerMovement.IncreaseSpeedServerRpc(true);

        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s";
            yield return new WaitForSeconds(1);
            if (i == 16)
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

    // test
    void OnCompleteTaskClicked()
    {
        var playerNetworkObject = NetworkManager.Singleton.LocalClient?.PlayerObject.GetComponent<NetworkObject>();
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            Debug.Log("Worker completed the task!");
            CompleteTaskServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void CompleteTaskServerRpc()
    {
        taskManager.CompleteTask();
    }
}

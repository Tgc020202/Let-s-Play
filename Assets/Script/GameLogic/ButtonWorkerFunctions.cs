using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ButtonWorkerFunctions : MonoBehaviour
{
    // UI Components
    private NetworkObject playerNetworkObject;
    private SpriteRenderer spriteRenderer;
    public Button runButton;
    public Button redButton;
    public Button greenButton;
    public Button completeTaskButton;   // test
    private Text runButtonText;

    // Scripts
    public PlayerMovement playerMovement;
    public TaskManager taskManager;
    private CollisionTriggerDisplay collisionTriggerDisplay;

    // GameObjects
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;
    private GameObject targetPlayer;

    // Defines
    private bool canUseRunButton = true;
    private bool canHelpPlayer = false;
    private float timer = 5f;

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
            spriteRenderer = playerNetworkObject.GetComponent<SpriteRenderer>();
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

    void OnRedButtonClicked()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            ChangePlayerColor(Color.red);
            SetImmunityServerRpc(playerNetworkObject.NetworkObjectId, true);
            playerMovement.enabled = false;

            GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(-1);

            // Task 1: Red Button Press
            taskManager.RedButtonPressed();
        }
    }

    void OnGreenButtonClicked()
    {
        targetPlayer = collisionTriggerDisplay.targetPlayer;
        canHelpPlayer = collisionTriggerDisplay.canHelpPlayer;

        ChangePlayerColor(Color.green);

        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canHelpPlayer && targetPlayer != null)
        {
            ChangePlayerColor(Color.green);
            HelpPlayerServerRpc(targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);

            GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(+1);
        }
    }

    void ChangePlayerColor(Color color)
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            var playerManager = playerNetworkObject.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                playerManager.SetPlayerColorServerRpc(color);
                if (color == Color.green)
                {
                    playerManager.ResetColorAfterDelay(2f);
                }
            }
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
                targetPlayerManager.SetPlayerColorServerRpc(Color.yellow);
                targetPlayerManager.ResetColorAfterDelay(2f);
            }
        }
    }

    // Testing
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

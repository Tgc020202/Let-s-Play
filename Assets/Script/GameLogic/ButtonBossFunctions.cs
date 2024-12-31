using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class ButtonBossFunctions : MonoBehaviour
{
    // UI Components
    private NetworkObject playerNetworkObject;
    public Button runButton;
    public Button catchButton;
    private Text runButtonText;
    private Text catchButtonText;

    // Scripts
    public PlayerMovement playerMovement;
    public CollisionTriggerDisplay collisionTriggerDisplay;

    // GameObjects
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;
    public GameObject BossControllerUI;
    private GameObject targetPlayer;

    // Defines
    private float timer = 20f;
    private bool canUseRunButton = true;
    private bool canCatchPlayer = false;

    // Messages
    private const string RunMessage = "Run";

    void Start()
    {
        GameViewUI.SetActive(false);
        MapDesign.SetActive(false);
        GuidanceUI.SetActive(true);

        runButton.onClick.AddListener(OnRunButtonClicked);
        catchButton.onClick.AddListener(OnCatchButtonClicked);
        runButtonText = runButton.GetComponentInChildren<Text>();
        catchButtonText = catchButton.GetComponentInChildren<Text>();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
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
            StartCoroutine(SpeedBoostCoolDown());
        }
    }

    void OnCatchButtonClicked()
    {
        targetPlayer = collisionTriggerDisplay.targetPlayer;
        canCatchPlayer = collisionTriggerDisplay.canCatchPlayer;

        StartCoroutine(CatchCoolDown(false));

        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canCatchPlayer && targetPlayer != null)
        {
            CatchPlayerServerRpc(targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
        }
    }

    IEnumerator SpeedBoostCoolDown()
    {
        canUseRunButton = false;
        playerMovement.IncreaseSpeedServerRpc(true, 20f);

        for (int i = 20; i > 0; i--)
        {
            runButtonText.text = i + "s";
            yield return new WaitForSeconds(1);
            if (i == 10)
            {
                playerMovement.IncreaseSpeedServerRpc(false, 20f);
            }
        }
        runButtonText.text = RunMessage;
        canUseRunButton = true;
    }

    IEnumerator CatchCoolDown(bool isActive)
    {
        int coolDownTime = isActive ? 5 : 3;

        catchButton.interactable = false;

        for (int i = coolDownTime; i > 0; i--)
        {
            catchButtonText.text = i + "s";
            yield return new WaitForSeconds(1);
        }
        catchButtonText.text = "Catch";
        catchButton.interactable = true;
    }

    void OnSkipGuidanceUI()
    {
        if (playerNetworkObject != null && playerNetworkObject.IsOwner)
        {
            SetBossImmunityServerRpc(playerNetworkObject.NetworkObjectId, BossControllerUI.activeSelf);
        }

        MapDesign.SetActive(true);
        GameViewUI.SetActive(true);
        GuidanceUI.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)]
    public void CatchPlayerServerRpc(ulong targetPlayerId)
    {
        var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];

        if (targetNetworkObject != null)
        {
            var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();

            if (targetPlayerManager != null && !targetPlayerManager.isBossRole && !targetPlayerManager.isImmuneToCatch && !targetPlayerManager.isSpectacle)
            {
                StartCoroutine(CatchCoolDown(true));

                targetPlayerManager.SetVisibilityServerRpc(false);
                targetPlayerManager.SetColliderServerRpc(false);

                GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(-1);
                targetPlayerManager.isSpectacle = true;
            }
        }
    }

    // Server RPC to set immunity for a player
    [ServerRpc(RequireOwnership = false)]
    public void SetBossImmunityServerRpc(ulong targetPlayerId, bool enabled)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.ContainsKey(targetPlayerId))
        {
            var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];

            if (targetNetworkObject != null)
            {
                var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();
                if (targetPlayerManager != null)
                {
                    targetPlayerManager.SetBossImmunityServerRpc(enabled);
                }
            }
        }
        else
        {
            Debug.Log($"NetworkObject with ID {targetPlayerId} does not exist.");
        }
    }
}

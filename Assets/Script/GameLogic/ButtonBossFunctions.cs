using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Netcode;

public class ButtonBossFunctions : MonoBehaviour
{
    private NetworkObject playerNetworkObject;
    public PlayerMovement playerMovement;
    public CollisionTriggerDisplay collisionTriggerDisplay;
    public Button runButton;
    public Button catchButton;
    private Text runButtonText;

    // Panels
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;
    private GameObject targetPlayer;

    private float timer = 20f;

    private bool canUseRunButton = true;
    private bool canCatchPlayer = false;


    void Start()
    {
        runButton.onClick.AddListener(OnRunButtonClicked);
        catchButton.onClick.AddListener(OnCatchButtonClicked);
        runButtonText = runButton.GetComponentInChildren<Text>();

        GameViewUI.SetActive(false);
        MapDesign.SetActive(false);
        GuidanceUI.SetActive(true);
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
            Debug.Log("Player " + playerNetworkObject.NetworkObjectId + " clicked the run button!");
            StartCoroutine(SpeedBoost());
        }
    }

    void OnCatchButtonClicked()
    {
        targetPlayer = collisionTriggerDisplay.targetPlayer;
        canCatchPlayer = collisionTriggerDisplay.canCatchPlayer;

        if (playerNetworkObject != null && playerNetworkObject.IsOwner && canCatchPlayer && targetPlayer != null)
        {
            Debug.Log("Catch attempt on player: " + targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
            CatchPlayerServerRpc(targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
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
    public void CatchPlayerServerRpc(ulong targetPlayerId)
    {
        var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];

        if (targetNetworkObject != null)
        {
            var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();

            if (targetPlayerManager != null && !targetPlayerManager.isImmuneToCatch)
            {
                targetPlayerManager.SetVisibilityServerRpc(false);
                targetPlayerManager.SetColliderServerRpc(false);

                Debug.Log("start run");
                // Decrease the worker count when a player is caught
                GameObject.FindObjectOfType<GameManager>()?.UpdateWorkerCountRequest(-1);
                Debug.Log("end run");
            }
        }
    }
}

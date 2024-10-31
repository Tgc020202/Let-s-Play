// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using Unity.Netcode;

// public class ButtonBossFunctions : MonoBehaviour
// {
//     private NetworkObject playerNetworkObject;
//     public PlayerMovement playerMovement;
//     public CollisionTriggerDisplay collisionTriggerDisplay;
//     public Button runButton;
//     public Button catchButton;
//     public GameObject GameViewUI;
//     public GameObject MapDesign;
//     public GameObject GuidanceUI;

//     private bool canUseRunButton = true;
//     private Text runButtonText;
//     private float timer = 5f;

//     private bool canCatchPlayer = false;
//     private GameObject targetPlayer;

//     void Start()
//     {
//         runButton.onClick.AddListener(OnRunButtonClicked);
//         catchButton.onClick.AddListener(OnCatchButtonClicked);
//         runButtonText = runButton.GetComponentInChildren<Text>();

//         GameViewUI.SetActive(false);
//         MapDesign.SetActive(false);
//         GuidanceUI.SetActive(true);
//         NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
//     }

//     void Update()
//     {
//         if (Input.GetKey(KeyCode.Space)) OnSkipGuidanceUI();

//         if (timer > 0)
//         {
//             timer -= Time.deltaTime;
//         }
//         else
//         {
//             OnSkipGuidanceUI();
//         }
//     }

//     private void OnClientConnected(ulong clientId)
//     {
//         if (NetworkManager.Singleton.LocalClient != null && NetworkManager.Singleton.LocalClient.PlayerObject != null)
//         {
//             playerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkObject>();
//             playerMovement = playerNetworkObject.GetComponent<PlayerMovement>();
//             collisionTriggerDisplay = playerNetworkObject.GetComponent<CollisionTriggerDisplay>();
//         }
//         else
//         {
//             Debug.LogError("Player object not found after spawn!");
//         }
//     }

//     void OnRunButtonClicked()
//     {
//         if (playerNetworkObject != null && playerNetworkObject.IsOwner && canUseRunButton)
//         {
//             Debug.Log("Player " + playerNetworkObject.NetworkObjectId + " clicked the run button!");
//             StartCoroutine(SpeedBoost());
//         }
//     }

//     void OnCatchButtonClicked()
//     {
//         targetPlayer = collisionTriggerDisplay.targetPlayer;
//         canCatchPlayer = collisionTriggerDisplay.canCatchPlayer;

//         Debug.Log("targetPlayer: " + targetPlayer);
//         Debug.Log("canCatchPlayer: " + canCatchPlayer);

//         if (playerNetworkObject != null && playerNetworkObject.IsOwner && canCatchPlayer && targetPlayer != null)
//         {
//             Debug.Log("Catch attempt on player: " + targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
//             CatchPlayerServerRpc(targetPlayer.GetComponent<NetworkObject>().NetworkObjectId);
//         }
//     }

//     [ServerRpc]
//     private void CatchPlayerServerRpc(ulong targetPlayerId)
//     {
//         Debug.Log("Executing CatchPlayerServerRpc...");
//         var targetNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[targetPlayerId];
//         if (targetNetworkObject != null)
//         {
//             // targetNetworkObject.Despawn();
//             // TODO: Dont use the Despawn, because it only allow server to do, lets use the not visible to the player
//             OnSpectateMode();
//         }
//     }

//     IEnumerator SpeedBoost()
//     {
//         canUseRunButton = false;
//         playerMovement.IncreaseSpeedServerRpc(true);

//         for (int i = 20; i > 0; i--)
//         {
//             runButtonText.text = i + "s";
//             yield return new WaitForSeconds(1);
//             if (i == 16)
//             {
//                 playerMovement.IncreaseSpeedServerRpc(false);
//             }
//         }
//         runButtonText.text = "Run";
//         canUseRunButton = true;
//     }

//     void OnSkipGuidanceUI()
//     {
//         MapDesign.SetActive(true);
//         GameViewUI.SetActive(true);
//         GuidanceUI.SetActive(false);
//     }

//     void OnSpectateMode(){
//         // TODO: All these effects are for the player that was caught
//         // cannot use AWSD to move the gameObject
//         // the camera of the player will stick with the player who caught him
//         // the game object of the player will be disappear and cannot view by other anymore
//         // the role text of the player become "Spectator"
//         // the WorkerButtonUI of the player closed
//     }
// }

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
    public GameObject GameViewUI;
    public GameObject MapDesign;
    public GameObject GuidanceUI;

    private bool canUseRunButton = true;
    private Text runButtonText;
    private float timer = 5f;

    private bool canCatchPlayer = false;
    private GameObject targetPlayer;

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

        Debug.Log("targetPlayer: " + targetPlayer);
        Debug.Log("canCatchPlayer: " + canCatchPlayer);

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
            Debug.Log($"Catching player: {targetPlayerId}");
            var targetPlayerManager = targetNetworkObject.GetComponent<PlayerManager>();
            if (targetPlayerManager != null)
            {
                // When player caught, allow move to view other players
                // targetPlayerManager.SetMovementEnabledServerRpc(false);
                targetPlayerManager.SetVisibilityServerRpc(false);
                targetPlayerManager.SetColliderServerRpc(false);
            }
        }
    }

}

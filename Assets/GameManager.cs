using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    public Text roleText;
    public GameObject BossControllerUI;
    public GameObject WorkerControllerUI;
    public GameObject BasicControllerUI;

    private NetworkVariable<int> numberOfBosses = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> numberOfWorkers = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, bool> rolesAssigned = new Dictionary<ulong, bool>(); // Track roles per client

    private bool isGameStart = false;
    private bool oneshot = true;

    void Update()
    {
        if (IsServer)
        {
            // To prevent the game assign first role as boss, and boss win directly
            if (numberOfWorkers.Value > 0 && oneshot)
            {
                isGameStart = true;
                oneshot = false;
            }

            // Check if the number of workers is zero and end the game if so
            if (numberOfWorkers.Value <= 0 && isGameStart)
            {
                GameObject.FindObjectOfType<GameViewTextBehaviour>()?.EndGameServerRpc(true); // Call end game
                EndGame();
            }
        }
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer || IsHost)
        {
            Debug.Log("Server is starting...");
            numberOfBosses.OnValueChanged += OnRoleCountChanged;
            numberOfWorkers.OnValueChanged += OnRoleCountChanged;

            AssignRoleLogic(NetworkManager.Singleton.LocalClientId);
        }

        if (IsClient && !IsHost)
        {
            RequestRoleAssignmentServerRpc();
        }
    }

    public void OnRoleCountChanged(int oldValue, int newValue)
    {
        Debug.Log("Number of Bosses: " + numberOfBosses.Value);
        Debug.Log("Number of Workers: " + numberOfWorkers.Value);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRoleAssignmentServerRpc(ServerRpcParams rpcParams = default)
    {
        AssignRoleLogic(rpcParams.Receive.SenderClientId);
    }

    private void AssignRoleLogic(ulong clientId)
    {
        if (!rolesAssigned.ContainsKey(clientId) || !rolesAssigned[clientId])
        {
            int randomRole = Random.Range(0, 2); // 0 = Worker, 1 = Boss
            // int randomRole = IsServer || IsHost ? 1 : 0; // Debug uses purpose
            Debug.Log("VariableHolder.maxNumberOfBosses: " + VariableHolder.maxNumberOfBosses);
            Debug.Log("VariableHolder.maxNumberOfWorkers: " + VariableHolder.maxNumberOfWorkers);

            if (randomRole == 1 && numberOfBosses.Value < VariableHolder.maxNumberOfBosses)
            {
                AssignBoss(clientId);
            }
            else if (numberOfWorkers.Value < VariableHolder.maxNumberOfWorkers)
            {
                AssignWorker(clientId);
            }
            else
            {
                Debug.Log("No available roles left for new client.");
            }
        }
        else
        {
            Debug.Log("Role already assigned for clientId: " + clientId); // For debugging
        }
    }

    private void AssignBoss(ulong clientId)
    {
        Debug.Log("Assigning Boss role to clientId: " + clientId);
        numberOfBosses.Value++;
        rolesAssigned[clientId] = true;
        UpdateRoleUIClientRpc(clientId, "Boss");
    }

    private void AssignWorker(ulong clientId)
    {
        Debug.Log("Assigning Worker role to clientId: " + clientId);
        numberOfWorkers.Value++;
        rolesAssigned[clientId] = true;
        UpdateRoleUIClientRpc(clientId, "Worker");
    }

    [ClientRpc]
    private void UpdateRoleUIClientRpc(ulong clientId, string role)
    {
        if (NetworkManager.Singleton.LocalClientId != clientId) return;

        roleText.text = role;
        BossControllerUI.SetActive(role == "Boss");
        WorkerControllerUI.SetActive(role == "Worker");
        BasicControllerUI.SetActive(true);

        Debug.Log("Updated UI for clientId: " + clientId + " with role: " + role);
    }

    public void UpdateWorkerCountRequest(int delta)
    {
        if (IsClient)
        {
            RequestWorkerCountUpdateServerRpc(delta);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestWorkerCountUpdateServerRpc(int delta)
    {
        // This code runs only on the server, updating the NetworkVariable
        numberOfWorkers.Value += delta;
    }

    // Clean data after the game end
    private void OnDestroy()
    {
        if (IsServer)
        {
            numberOfBosses.OnValueChanged -= OnRoleCountChanged;
            numberOfWorkers.OnValueChanged -= OnRoleCountChanged;
        }
    }

    [ServerRpc]
    private void DespawnPlayerServerRpc(ulong clientId)
    {
        var playerObject = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        if (playerObject != null)
        {
            playerObject.Despawn();
            Debug.Log($"Despawned player object for clientId: {clientId}");
        }
    }
    public void EndGame()
    {
        if (IsServer)
        {
            foreach (var playerObject in NetworkManager.Singleton.ConnectedClientsList)
            {
                if (playerObject.PlayerObject != null)
                {
                    // Call an RPC to despawn the player object
                    DespawnPlayerServerRpc(playerObject.ClientId);
                }
            }

            // Reset roles and counts
            rolesAssigned.Clear();
            numberOfBosses.Value = 0;
            numberOfWorkers.Value = 0;


            // Set the flag to indicate the player is returning to the room
            VariableHolder.isFromEndGameToRoom = true;

            // Optionally, you can despawn the GameManager itself if needed
            NetworkManager.Singleton.Shutdown();
        }
    }
}
